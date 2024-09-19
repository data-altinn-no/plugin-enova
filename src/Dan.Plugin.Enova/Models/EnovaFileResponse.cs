using System;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Models;

[Serializable]
public class EnovaFileResponse
{
    [JsonProperty(PropertyName = "fromDate")]
    public DateTime FromDate { get; set; }

    [JsonProperty(PropertyName = "toDate")]
    public DateTime ToDate { get; set; }

    [JsonProperty(PropertyName = "bankFileUrl")]
    public string FilePath { get; set; }
}
