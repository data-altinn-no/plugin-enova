using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dan.Plugin.Enova.Extensions;
using Dan.Plugin.Enova.Models;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Test.Extensions;

public class DistributedCacheExtensionsTests
{
    private readonly Mock<IDistributedCache> _distributedCache = new();

    [Fact]
    public async Task GetValueAsync_Bool_ValueIsNull_ShouldBeFalse()
    {
        // Arrange
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as byte[]);

        // Act
        var actual = await _distributedCache.Object.GetValueAsync<bool>("dummy");

        // Assert
        actual.Should().Be(false);
    }

    [Fact]
    public async Task GetValueAsync_Bool_ValueFound_ShouldBeTrue()
    {
        // Arrange
        var trueByteArray = "true"u8.ToArray();
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(trueByteArray);

        // Act
        var actual = await _distributedCache.Object.GetValueAsync<bool>("dummy");

        // Assert
        actual.Should().Be(true);
    }

    [Fact]
    public async Task GetValueAsync_EmsCsvList_ValueIsNull_ShouldBeNull()
    {
        // Arrange
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as byte[]);

        // Act
        var actual = await _distributedCache.Object.GetValueAsync<List<EmsCsv>>("dummy");

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetValueAsync_EmsCsvList_ValueFound_ShouldBeList()
    {
        // Arrange
        List<EmsCsv>  emscsvlist = [new EmsCsv { Organisasjonsnummer = "123" }];
        var serializedValue = JsonConvert.SerializeObject(emscsvlist);
        var emscsvlistBytes = Encoding.UTF8.GetBytes(serializedValue);
        _distributedCache
            .Setup(cache => cache.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emscsvlistBytes);

        // Act
        var actual = await _distributedCache.Object.GetValueAsync<List<EmsCsv>>("dummy");

        // Assert
        actual.Should().BeEquivalentTo(emscsvlist);
    }
}
