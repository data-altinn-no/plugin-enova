using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Common.Util;
using Dan.Plugin.Enova.Clients;
using Dan.Plugin.Enova.Config;
using Dan.Plugin.Enova.Mappers;
using Dan.Plugin.Enova.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dan.Plugin.Enova;

public class Plugin
{
    private readonly IEntityRegistryService _entityRegistryService;
    private readonly IEvidenceSourceMetadata _evidenceSourceMetadata;
    private readonly IEnovaClient _enovaClient;
    private readonly IMapper<EmsCsv, EmsResponseModel> _mapper;
    private readonly ILogger _logger;
    private readonly HttpClient _client;
    private readonly Settings _settings;

    public Plugin(
        IHttpClientFactory httpClientFactory,
        IEntityRegistryService entityRegistryService,
        ILoggerFactory loggerFactory,
        IOptions<Settings> settings,
        IEvidenceSourceMetadata evidenceSourceMetadata,
        IEnovaClient enovaClient,
        IMapper<EmsCsv, EmsResponseModel> mapper)
    {
        _client = httpClientFactory.CreateClient(Constants.SafeHttpClient);
        _logger = loggerFactory.CreateLogger<Plugin>();
        _settings = settings.Value;
        _entityRegistryService = entityRegistryService;
        _evidenceSourceMetadata = evidenceSourceMetadata;
        _enovaClient = enovaClient;
        _mapper = mapper;

        _logger.LogDebug("Initialized plugin! This should be visible in the console");
    }

    [Function(PluginConstants.PublicEnergyData)]
    public async Task<HttpResponseData> GetSimpleDatasetAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        EvidenceHarvesterRequest evidenceHarvesterRequest;
        try
        {
            evidenceHarvesterRequest = await req.ReadFromJsonAsync<EvidenceHarvesterRequest>();
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Exception while attempting to parse request into EvidenceHarvesterRequest: {exceptionType}: {exceptionMessage}",
                e.GetType().Name, e.Message);
            throw new EvidenceSourcePermanentClientException(PluginConstants.ErrorInvalidInput,
                "Unable to parse request", e);
        }

        return await EvidenceSourceResponse.CreateResponse(req,
            () => GetEvidenceValuesPublicEmsData(evidenceHarvesterRequest));
    }

    // While RunOnStartup is normally not recommended for production settings, as this makes it unpredictable
    // when the function will trigger and can for more expensive functions in terms of what they do inflate costs,
    // here it will just find the values in the cache and be done. If the cache is empty, then we want to repopulate it
    // as quickly as possible either way. Running it once per day just ensures that the current year stays up to date
    // as it gets invalidated after a day
    [Function(nameof(RunCache))]
    public async Task RunCache(
        [TimerTrigger("0 30 3 * * *", RunOnStartup = true)] TimerInfo timerInfo,
        FunctionContext context)
    {
        foreach (var year in GetLastFiveYears())
        {
            // We don't care about what org to filter on, just want to fill the cache
            await _enovaClient.GetEnergyPublicData(year, string.Empty);
        }
    }

    private async Task<List<EvidenceValue>> GetEvidenceValuesPublicEmsData(
        EvidenceHarvesterRequest evidenceHarvesterRequest)
    {
        if (evidenceHarvesterRequest?.OrganizationNumber is null)
        {
            throw new EvidenceSourcePermanentClientException(PluginConstants.ErrorInvalidInput,
                "Request is missing organization number");
        }

        var entity = await _entityRegistryService.GetFull(evidenceHarvesterRequest.OrganizationNumber);
        if (entity is null)
        {
            throw new EvidenceSourcePermanentClientException(PluginConstants.ErrorNotFound,
                $"Legal entity ({evidenceHarvesterRequest.OrganizationNumber})not found");
        }

        var dict = new Dictionary<int, List<EmsResponseModel>>();
        foreach (var year in GetLastFiveYears())
        {
            // We don't care about what org to filter on, just want to fill the cache
            var csv = await _enovaClient.GetEnergyPublicData(year, entity.Organisasjonsnummer);
            var responseModels = csv.Select(_mapper.Map).ToList();
            dict.Add(year, responseModels);
        }

        var response = dict
            .Where(kvp => kvp.Value.Count != 0)
            .OrderByDescending(kvp => kvp.Key);
        var ecb = new EvidenceBuilder(_evidenceSourceMetadata, PluginConstants.PublicEnergyData);
        ecb.AddEvidenceValue("default", response, PluginConstants.SourceName);

        return ecb.GetEvidenceValues();
    }

    private static IEnumerable<int> GetLastFiveYears()
    {
        var currentYear = DateTime.UtcNow.Year;
        var lastFiveYears = Enumerable.Range(currentYear-4, 5);
        return lastFiveYears;
    }
}
