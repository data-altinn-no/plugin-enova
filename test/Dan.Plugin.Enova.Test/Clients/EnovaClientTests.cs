using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dan.Plugin.Enova.Clients;
using Dan.Plugin.Enova.Config;
using Dan.Plugin.Enova.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Test.Clients;

public class EnovaClientTests
{
    private readonly Mock<IDistributedCache> _distributedCacheMock;

    private readonly EnovaClient _enovaClient;

    public EnovaClientTests()
    {
        Mock<IHttpClientFactory> clientFactoryMock = new();
        Mock<IOptions<Settings>> settingsMock = new();

        Mock<ILoggerFactory> loggerFactoryMock = new();

        _distributedCacheMock = new Mock<IDistributedCache>();
        _enovaClient = new EnovaClient(
            clientFactoryMock.Object,
            settingsMock.Object,
            loggerFactoryMock.Object,
            _distributedCacheMock.Object);
    }

    [Fact]
    public async Task GetEnergyPublicData_YearIsCached_NoValueFound_ShouldReturnEmptyList()
    {
        // Arrange
        const int year = 2023;
        const string organizationNumber = "org";
        var trueArray = "true"u8.ToArray();
        var isCachedKey = $"{PluginConstants.SourceName}-EmsCsv-{year}-IsCached";
        var cacheValueKey = $"{PluginConstants.SourceName}-EmsCsv-{organizationNumber}-{year}";

        _distributedCacheMock
            .Setup(m => m.GetAsync(isCachedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trueArray);

        _distributedCacheMock
            .Setup(m => m.GetAsync(cacheValueKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as byte[]);

        // Act
        var actual = await _enovaClient.GetEnergyPublicData(year, organizationNumber);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetEnergyPublicData_YearIsCached_ValueFound_ShouldEmsCsv()
    {
        // Arrange
        const int year = 2023;
        const string organizationNumber = "org";

        var trueArray = "true"u8.ToArray();
        var isCachedKey = $"{PluginConstants.SourceName}-EmsCsv-{year}-IsCached";
        _distributedCacheMock
            .Setup(m => m.GetAsync(isCachedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trueArray);

        IEnumerable<EmsCsv> emsCsv = new List<EmsCsv> { new() { Organisasjonsnummer = organizationNumber } };

        var cacheValueKey = $"{PluginConstants.SourceName}-EmsCsv-{organizationNumber}-{year}";
        _distributedCacheMock
            .Setup(m => m.GetAsync(cacheValueKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ToByteArray(emsCsv));

        // Act
        var actual = await _enovaClient.GetEnergyPublicData(year, organizationNumber);

        // Assert
        actual.Should().BeEquivalentTo(emsCsv);
    }

    [Fact]
    public async Task CachePerOrganization_ShouldCacheYearAndValues()
    {
        // Arrange
        const int year = 2023;
        List<EmsCsv> records =
        [
            new EmsCsv
            {
                Organisasjonsnummer = "1",
                Andelsnummer = "1-1"
            },
            new EmsCsv
            {
                Organisasjonsnummer = "1",
                Andelsnummer = "1-2"
            },
            new EmsCsv
            {
                Organisasjonsnummer = "2",
                Andelsnummer = "2-1"
            }
        ];

        // Act
        await _enovaClient.CachePerOrganization(year, records);

        // Assert
        _distributedCacheMock
            .Verify(c =>
                c.SetAsync("Enova-EmsCsv-2023-IsCached", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);

        _distributedCacheMock
            .Verify(c =>
                    c.SetAsync("Enova-EmsCsv-1-2023", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);

        _distributedCacheMock
            .Verify(c =>
                    c.SetAsync("Enova-EmsCsv-2-2023", It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()),
                Times.Once);
    }

    private static byte[] ToByteArray<T>(T value)
    {
        var json = JsonConvert.SerializeObject(value);
        return Encoding.UTF8.GetBytes(json);
    }
}
