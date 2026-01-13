using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dan.Plugin.Enova.Extensions;
using Dan.Plugin.Enova.Models;
using FakeItEasy;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace Dan.Plugin.Enova.Test.Extensions;

public class DistributedCacheExtensionsTests
{
    private readonly IDistributedCache _distributedCache = A.Fake<IDistributedCache>();

    [Fact]
    public async Task GetValueAsync_Bool_ValueIsNull_ShouldBeFalse()
    {
        // Arrange
        A.CallTo(() => _distributedCache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
         .Returns(null as byte[]);

        // Act
        var actual = await _distributedCache.GetValueAsync<bool>("dummy");

        // Assert
        actual.Should().Be(false);
    }

    [Fact]
    public async Task GetValueAsync_Bool_ValueFound_ShouldBeTrue()
    {
        // Arrange
        var trueByteArray = "true"u8.ToArray();
        A.CallTo(() => _distributedCache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
         .Returns(trueByteArray);

        // Act
        var actual = await _distributedCache.GetValueAsync<bool>("dummy");

        // Assert
        actual.Should().Be(true);
    }

    [Fact]
    public async Task GetValueAsync_EmsCsvList_ValueIsNull_ShouldBeNull()
    {
        // Arrange
        A.CallTo(() => _distributedCache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
         .Returns(null as byte[]);

        // Act
        var actual = await _distributedCache.GetValueAsync<List<EmsCsv>>("dummy");

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetValueAsync_EmsCsvList_ValueFound_ShouldBeList()
    {
        // Arrange
        List<EmsCsv>  emscsvlist = [new() { Organisasjonsnummer = "123" }];
        var serializedValue = JsonConvert.SerializeObject(emscsvlist);
        var emscsvlistBytes = Encoding.UTF8.GetBytes(serializedValue);
        A.CallTo(() => _distributedCache.GetAsync(A<string>.Ignored, A<CancellationToken>.Ignored))
         .Returns(emscsvlistBytes);

        // Act
        var actual = await _distributedCache.GetValueAsync<List<EmsCsv>>("dummy");

        // Assert
        actual.Should().BeEquivalentTo(emscsvlist);
    }
}
