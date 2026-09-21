using Azure.Core;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.Hosting;
using Dan.Common.Extensions;
using Dan.Plugin.Enova.Clients;
using Dan.Plugin.Enova.Config;
using Dan.Plugin.Enova.Mappers;
using Dan.Plugin.Enova.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using StackExchange.Redis;

var host = new HostBuilder()
    .ConfigureDanPluginDefaults()
    .ConfigureAppConfiguration((_, _) =>
    {
        // Add more configuration sources if necessary. ConfigureDanPluginDefaults will load environment variables, which includes
        // local.settings.json (if developing locally) and applications settings for the Azure Function
    })
    .ConfigureLogging(logging =>
    {
        logging.AddOpenTelemetry(options =>
        {
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });
    })
    .ConfigureServices((context, services) =>
    {
        // native OpenTelemetry running alongside the classic AI SDK from ConfigureDanPluginDefaults() in Dan.Common.
        // We will get double logging in Application Insights for now, but this is a POC to validate that OpenTelemetry works before we remove the classic AI SDK in Dan.Common.
        var appInsightsConnectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        if (!string.IsNullOrWhiteSpace(appInsightsConnectionString))
        {
            services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddHttpClientInstrumentation())
                .UseAzureMonitorExporter(options => options.ConnectionString = appInsightsConnectionString)
                .UseFunctionsWorkerDefaults();
        }

        // Add any additional services here
        services.AddTransient<IEnovaClient, EnovaClient>();
        services.AddTransient<IMapper<EmsCsv, EmsResponseModel>, EmsResponseModelMapper>();

        // This makes IOption<Settings> available in the DI container.
        var configurationRoot = context.Configuration;
        services.Configure<Settings>(configurationRoot);

        var applicationSettings = services.BuildServiceProvider().GetRequiredService<IOptions<Settings>>().Value;
        // In case of still using access key (or local redis),
        TokenCredential credential = new DefaultAzureCredential();
        if (applicationSettings.RedisConnectionString.Contains("password=") ||
            applicationSettings.RedisConnectionString.Contains("127.0.0.1"))
        {
            services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = applicationSettings.RedisConnectionString;
            });
        }
        else
        {
            services.AddStackExchangeRedisCache(option =>
            {
                option.ConnectionMultiplexerFactory = async () =>
                {
                    var configurationOptions = await ConfigurationOptions
                        .Parse(applicationSettings.RedisConnectionString)
                        .ConfigureForAzureWithTokenCredentialAsync(credential);

                    var connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

                    return connectionMultiplexer;
                };
            });
        }
    })
    .Build();

await host.RunAsync();
