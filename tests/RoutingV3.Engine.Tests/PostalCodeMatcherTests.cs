using RoutingV3.Domain.Models;
using RoutingV3.Engine;

namespace RoutingV3.Engine.Tests;

public class PostalCodeMatcherTests
{
    private readonly PostalCodeMatcher _matcher = new();

    private static List<PostalCodeArea> GetTestAreas() =>
    [
        new PostalCodeArea { Id = 1, Country = "AT", Pattern = "4.*" },
        new PostalCodeArea { Id = 2, Country = "AT", Pattern = "1.*" },
        new PostalCodeArea { Id = 3, Country = "DE", Pattern = "4.*", SubHubCode = "DE-41379" },
        new PostalCodeArea { Id = 4, Country = "DE", Pattern = "1.*", SubHubCode = "DE-10115" },
        new PostalCodeArea { Id = 5, Country = "DK", Pattern = "2[0-9]{3}" }
    ];

    [Theory]
    [InlineData("AT", "4020", 1)]    // Linz postal code matches AT-4*
    [InlineData("AT", "4600", 1)]    // Another 4xxx
    [InlineData("AT", "1010", 2)]    // Vienna matches AT-1*
    [InlineData("DE", "40210", 3)]   // Düsseldorf matches DE-4*
    [InlineData("DE", "10115", 4)]   // Berlin matches DE-1*
    [InlineData("DK", "2100", 5)]    // Copenhagen matches DK-2xxx
    public void FindMatches_ValidPostalCode_ReturnsCorrectArea(string country, string postalCode, int expectedId)
    {
        var matches = _matcher.FindMatches(country, postalCode, GetTestAreas());
        Assert.Single(matches);
        Assert.Equal(expectedId, matches[0].Id);
    }

    [Fact]
    public void FindMatches_NoMatch_ReturnsEmpty()
    {
        var matches = _matcher.FindMatches("AT", "9999", GetTestAreas());
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_WrongCountry_ReturnsEmpty()
    {
        var matches = _matcher.FindMatches("FR", "4020", GetTestAreas());
        Assert.Empty(matches);
    }

    [Fact]
    public void FindMatches_MultipleMatches_ReturnsAll()
    {
        var areas = new List<PostalCodeArea>
        {
            new() { Id = 1, Country = "AT", Pattern = "4.*" },
            new() { Id = 2, Country = "AT", Pattern = "40.*" },
        };
        var matches = _matcher.FindMatches("AT", "4020", areas);
        Assert.Equal(2, matches.Count);
    }

    [Fact]
    public void FindMatches_CaseInsensitiveCountry()
    {
        var matches = _matcher.FindMatches("at", "4020", GetTestAreas());
        Assert.Single(matches);
    }

    [Fact]
    public void IsMatch_ExactPattern()
    {
        Assert.True(_matcher.IsMatch("4020", "4020"));
    }

    [Fact]
    public void IsMatch_WildcardPattern()
    {
        Assert.True(_matcher.IsMatch("4020", "4.*"));
        Assert.False(_matcher.IsMatch("5020", "4.*"));
    }

    [Fact]
    public void IsMatch_InvalidRegex_ReturnsFalse()
    {
        Assert.False(_matcher.IsMatch("4020", "[invalid"));
    }
}
