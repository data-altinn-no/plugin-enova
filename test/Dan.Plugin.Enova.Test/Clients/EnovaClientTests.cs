using System;
using System.Collections.Generic;
using System.Linq;
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
using RichardSzalay.MockHttp;

namespace Dan.Plugin.Enova.Test.Clients;

public class EnovaClientTests
{
    private readonly MockHttpMessageHandler _mockHttpHandler;
    private readonly Mock<IDistributedCache> _distributedCacheMock;
    private readonly EnovaClient _enovaClient;

    private  const string BaseAddress = "https://mock.mock-enova.mock";

    public EnovaClientTests()
    {
        _mockHttpHandler  = new MockHttpMessageHandler();
        var settings = new Settings{EnovaUrl = BaseAddress};
        Mock<IHttpClientFactory> clientFactoryMock = new();
        Mock<IOptions<Settings>> settingsOptions = new();
        Mock<ILoggerFactory> loggerFactoryMock = new();

        _distributedCacheMock = new Mock<IDistributedCache>();

        clientFactoryMock
            .Setup(x => x.CreateClient(It.IsAny<string>()))
            .Returns(_mockHttpHandler.ToHttpClient());

        settingsOptions.Setup(x => x.Value).Returns(settings);

        _enovaClient = new EnovaClient(
            clientFactoryMock.Object,
            settingsOptions.Object,
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
    public async Task GetEnergyPublicData_CacheNotFound_ShouldFetchFromSource()
    {
        // Arrange
        const int year = 2023;
        const string organizationNumber = "org";
        const string mockFilePath = "https://mock.mock-enova.mock/mock-file-path";
        var fullUrl = $"{BaseAddress}/ems/offentlige-data/v1/Fil/{year}";

        var falseArray = "false"u8.ToArray();
        var isCachedKey = $"{PluginConstants.SourceName}-EmsCsv-{year}-IsCached";
        _distributedCacheMock
            .Setup(m => m.GetAsync(isCachedKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(falseArray);

        const string csv = "Knr,Gnr,Bnr,Snr,Fnr,Andelsnummer,Bygningsnummer,GateAdresse,Postnummer,Poststed,BruksEnhetsNummer,Organisasjonsnummer,Bygningskategori,Byggear,Energikarakter,Oppvarmingskarakter,Utstedelsesdato,TypeRegistrering,Attestnummer,BeregnetLevertEnergiTotaltkWhm2,BeregnetFossilandel,Materialvalg,HarEnergiVurdering,EnergiVurderingDato\n" +
                           "1234,11,22,33,44,,11223344,Mockvei 123,1234,MOCK,H0101,org,Småhus,1976,E,Yellow,2023-12-31T23:45:20.0000000,Simple,randomguidgoeshere,224.93,\"0,61\",Tre,False,";
        _mockHttpHandler
            .When(fullUrl)
            .Respond("application/json", $"{{'bankFileUrl': '{mockFilePath}'}}");

        _mockHttpHandler
            .When(mockFilePath)
            .Respond("application/octet-stream", csv);

        var expected = new EmsResponseModel
        {
            Kommunenummer = 1234,
            Gaardsnummer = "11",
            Bruksnummer = "22",
            Seksjonsnummer = "33",
            Festenummer = "44",
            Andelsnummer = "",
            Bygningsnummer = "11223344",
            GateAdresse = "Mockvei 123",
            Postnummer = 1234,
            Poststed = "MOCK",
            BruksEnhetsNummer = "H0101",
            Organisasjonsnummer = "org",
            Bygningskategori = "Småhus",
            Byggear = 1976,
            Energikarakter = "E",
            Oppvarmingskarakter = "Yellow",
            Utstedelsesdato = DateTime.Parse("2023-12-31T23:45:20.0000000"),
            TypeRegistrering = "Simple",
            Attestnummer = "randomguidgoeshere",
            BeregnetLevertEnergiTotaltkWhm2 = 224.93,
            BeregnetFossilandel = "0,61",
            Materialvalg = "Tre",
            HarEnergiVurdering = false,
            EnergiVurderingDato = null
        };

        // Act
        var actual = await _enovaClient.GetEnergyPublicData(year, organizationNumber);

        // Assert
        actual.FirstOrDefault().Should().BeEquivalentTo(expected);
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
