using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Dan.Common;
using Dan.Common.Exceptions;
using Dan.Plugin.Enova.Config;
using Dan.Plugin.Enova.Extensions;
using Dan.Plugin.Enova.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Clients;

public interface IEnovaClient
{
    Task<IEnumerable<EmsCsv>> GetEnergyPublicData(int year, string organizationNumber, bool forceRefresh = false);
}

public class EnovaClient(
    IHttpClientFactory clientFactory,
    IOptions<Settings> settings,
    ILoggerFactory loggerFactory,
    IDistributedCache distributedCache) : IEnovaClient
{
    private readonly HttpClient _client = clientFactory.CreateClient(Constants.SafeHttpClient);
    private readonly Settings _settings = settings.Value;
    private readonly ILogger<EnovaClient> _logger = loggerFactory.CreateLogger<EnovaClient>();

    public async Task<IEnumerable<EmsCsv>> GetEnergyPublicData(int year, string organizationNumber, bool forceRefresh = false)
    {
        var cacheKey = GetOrganizationEmsCsvCacheKey(year, organizationNumber);
        var isCachedKey = GetYearCacheKey(year);

        // An org might not have values stored for every year, so we want to avoid doing another lookup if
        // we have already cached queried year
        var isYearCached = await distributedCache.GetValueAsync<bool>(isCachedKey);
        if (!forceRefresh && isYearCached)
        {
            var cachedValue = await distributedCache.GetValueAsync<IEnumerable<EmsCsv>>(cacheKey);
            return cachedValue != null ? cachedValue.ToList() : [];
        }

        var baseAddress = _settings.EnovaUrl;
        var path = $"/ems/offentlige-data/v1/Fil/{year}";

        var request = GetRequest($"{baseAddress}{path}");
        var response = await MakeRequest<EnovaFileResponse>(request);
        var fileUrl = response.FilePath;

        var fileRequest = GetRequest(fileUrl);
        var fileResponse = await _client.SendAsync(fileRequest);

        var records = await GetCsvRecordsFromHttpResponse(fileResponse);
        await CachePerOrganization(year, records);
        return records.Where(csv => csv.Organisasjonsnummer == organizationNumber).ToList();
    }

    private HttpRequestMessage GetRequest(string url)
    {
        var uriBuilder = new UriBuilder(url);
        var uri = uriBuilder.ToString();
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.TryAddWithoutValidation("x-api-key", _settings.ApiKey);

        return request;
    }

    private async Task<T> MakeRequest<T>(HttpRequestMessage request)
    {
        HttpResponseMessage result;
        try
        {
            result = await _client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            throw new EvidenceSourceTransientException(PluginConstants.ErrorUpstreamUnavailble, "Error communicating with upstream source", ex);
        }

        if (!result.IsSuccessStatusCode)
        {
            throw result.StatusCode switch
            {
                HttpStatusCode.NotFound => new EvidenceSourcePermanentClientException(PluginConstants.ErrorNotFound, "Upstream source could not find the requested entity (404)"),
                HttpStatusCode.BadRequest => new EvidenceSourcePermanentClientException(PluginConstants.ErrorInvalidInput,  "Upstream source indicated an invalid request (400)"),
                _ => new EvidenceSourceTransientException(PluginConstants.ErrorUpstreamUnavailble, $"Upstream source retuned an HTTP error code ({(int)result.StatusCode})")
            };
        }

        try
        {
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError("Unable to parse data returned from upstream source: {exceptionType}: {exceptionMessage}", ex.GetType().Name, ex.Message);
            throw new EvidenceSourcePermanentServerException(PluginConstants.ErrorUnableToParseResponse, "Could not parse the data model returned from upstream source", ex);
        }
    }

    private async Task<List<EmsCsv>> GetCsvRecordsFromHttpResponse(HttpResponseMessage response)
    {
        try
        {
            var stream = await response.Content.ReadAsStreamAsync();
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Quote = '"'
            };
            using var reader = new StreamReader(stream);
            using var csvReader = new CsvReader(reader, csvConfig);
            var records = csvReader.GetRecords<EmsCsv>()
                .Where(csv => !string.IsNullOrEmpty(csv.Organisasjonsnummer))
                .ToList();

            return records;
        }
        catch (Exception e)
        {
            _logger.LogError("Unable to read csv from response: {exceptionType}: {exceptionMessage}", e.GetType().Name, e.Message);
            throw;
        }
    }

    public async Task CachePerOrganization(int year, List<EmsCsv> records)
    {
        // Set this first so that it gets invalidated first
        var isCachedKey = GetYearCacheKey(year);
        await CacheValues(year, isCachedKey, true);

        var groupedByOrg = records.GroupBy(record => record.Organisasjonsnummer);
        foreach (var organization in groupedByOrg)
        {
            var orgRecords = organization.ToList();
            // Some org numbers are formated like "### ### ###", so need to remove that whitespace
            var cacheKey = GetOrganizationEmsCsvCacheKey(year, organization.Key.TrimAllWhitespace());
            await CacheValues(year, cacheKey, orgRecords);
        }
    }

    private async Task CacheValues<T>(int year, string cacheKey, T value)
    {
        var cacheExpiration = DateTime.UtcNow.Year == year ?
            TimeSpan.FromDays(1) :
            TimeSpan.FromDays(365);
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = cacheExpiration
        };
        await distributedCache.SetValueAsync(cacheKey, value, options);
    }

    private static string GetOrganizationEmsCsvCacheKey(int year, string organizationNumber)
        => $"{PluginConstants.SourceName}-EmsCsv-{organizationNumber}-{year}";
    private static string GetYearCacheKey(int year) => $"{PluginConstants.SourceName}-EmsCsv-{year}-IsCached";
}
