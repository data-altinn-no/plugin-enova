using Dan.Plugin.Enova.Extensions;
using Xunit;

namespace Dan.Plugin.Enova.Test.Extensions;

public class StringExtensionTests
{
    [Theory]
    [InlineData("HelloWorld", "HelloWorld")]
    [InlineData("Hello World", "HelloWorld")]
    [InlineData(" Hello World ", "HelloWorld")]
    [InlineData("   Hello       World ", "HelloWorld")]
    public void TrimAllWhitspace_ShouldTrim(string input, string expected)
    {
        // Act
        var actual = input.TrimAllWhitespace();

        // Assert
        actual.Should().Be(expected);
    }
}
