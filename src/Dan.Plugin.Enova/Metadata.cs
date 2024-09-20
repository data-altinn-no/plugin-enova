using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Dan.Common;
using Dan.Common.Enums;
using Dan.Common.Interfaces;
using Dan.Common.Models;
using Dan.Plugin.Enova.Config;
using Dan.Plugin.Enova.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using NJsonSchema;

namespace Dan.Plugin.Enova;

/// <summary>
/// All plugins must implement IEvidenceSourceMetadata, which describes that datasets returned by this plugin. An example is implemented below.
/// </summary>
public class Metadata : IEvidenceSourceMetadata
{
    [Function(Constants.EvidenceSourceMetadataFunctionName)]
    public async Task<HttpResponseData> GetMetadataAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
        FunctionContext context)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(GetEvidenceCodes());
        return response;
    }

    public List<EvidenceCode> GetEvidenceCodes()
    {
        return
        [
            new EvidenceCode
            {
                EvidenceCodeName = PluginConstants.PublicEnergyData,
                EvidenceSource = PluginConstants.SourceName,
                ServiceContext = "eDueDiligence",
                Values =
                [
                    new EvidenceValue
                    {
                        EvidenceValueName = "default",
                        ValueType = EvidenceValueType.JsonSchema,
                        JsonSchemaDefintion = JsonSchema
                            .FromType<Dictionary<int, EmsResponseModel>>()
                            .ToJson(Newtonsoft.Json.Formatting.Indented)
                    }
                ]
            }
        ];
    }
}
