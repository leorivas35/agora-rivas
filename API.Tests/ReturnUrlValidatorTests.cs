using API.Auth;
using Microsoft.Extensions.Configuration;

namespace API.Tests;

public class ReturnUrlValidatorTests
{
    private static ReturnUrlValidator CreateValidator() => new(new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Authentication:AllowedReturnOrigins:0"] = "https://app.example.test"
        })
        .Build());

    [Theory]
    [InlineData("/")]
    [InlineData("/library?tab=music")]
    [InlineData("https://app.example.test/callback")]
    [InlineData("https://APP.example.test:443/callback")]
    public void IsAllowed_ReturnsTrue_ForLocalOrAllowListedUrls(string returnUrl)
    {
        Assert.True(CreateValidator().IsAllowed(returnUrl));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("//evil.example")]
    [InlineData("/\\evil.example")]
    [InlineData("https://evil.example/")]
    [InlineData("https://app.example.test.evil.example/")]
    [InlineData("javascript:alert(1)")]
    [InlineData("relative/path")]
    public void IsAllowed_ReturnsFalse_ForUnsafeUrls(string? returnUrl)
    {
        Assert.False(CreateValidator().IsAllowed(returnUrl));
    }
}
