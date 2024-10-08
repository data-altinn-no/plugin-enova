namespace Dan.Plugin.Enova.Config;

public class Settings
{
    public int DefaultCircuitBreakerOpenCircuitTimeSeconds { get; init; }
    public int DefaultCircuitBreakerFailureBeforeTripping { get; init; }
    public int SafeHttpClientTimeout { get; init; }

    public string RedisConnectionString { get; init; }

    public string EnovaUrl { get; init; }
    public string ApiKey { get; init; }
}
