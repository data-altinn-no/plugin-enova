using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Models;

public class ExampleModel
{
    [JsonRequired]
    public string ResponseField1 { get; set; }

    [JsonRequired]
    public string ResponseField2 { get; set; }
}
