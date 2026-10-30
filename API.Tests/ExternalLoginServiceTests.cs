using System.Security.Claims;
using API.Auth;
using Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests;

public class ExternalLoginServiceTests : IClassFixture<AuthApiFactory>
{
    private readonly AuthApiFactory factory;

    public ExternalLoginServiceTests(AuthApiFactory factory)
    {
        this.factory = factory;
    }

    private static ExternalLoginInfo CreateLoginInfo(string providerKey, string? email, bool emailVerified)
    {
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, providerKey) };
        if (email is not null)
        {
            claims.Add(new Claim(ClaimTypes.Email, email));
        }

        claims.Add(new Claim(ExternalClaimTypes.EmailVerified, emailVerified ? "true" : "false"));

        return new ExternalLoginInfo(
            new ClaimsPrincipal(new ClaimsIdentity(claims, "External")),
            "Google",
            providerKey,
            "Google");
    }

    [Fact]
    public async Task ResolveUserAsync_CreatesUser_WhenEmailIsVerified()
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IExternalLoginService>();

        var result = await service.ResolveUserAsync(CreateLoginInfo("new-key", "new@example.test", true));

        Assert.True(result.Succeeded);
        Assert.Equal("new@example.test", result.User?.Email);
        Assert.True(result.User?.EmailConfirmed);

        var again = await service.ResolveUserAsync(CreateLoginInfo("new-key", null, false));
        Assert.Equal(result.User?.Id, again.User?.Id);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResolveUserAsync_Rejects_WhenEmailIsMissingOrUnverified(bool includeEmail)
    {
        using var scope = factory.Services.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<IExternalLoginService>();
        var email = includeEmail ? "unverified@example.test" : null;

        var result = await service.ResolveUserAsync(CreateLoginInfo($"bad-{includeEmail}", email, false));

        Assert.False(result.Succeeded);
        Assert.Equal(ExternalLoginFailure.VerifiedEmailRequired, result.Failure);
    }

    [Fact]
    public async Task ResolveUserAsync_LinksExistingUser_ByVerifiedEmail()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Profile>>();
        var existing = new Profile { UserName = "link@example.test", Email = "link@example.test", EmailConfirmed = true };
        Assert.True((await userManager.CreateAsync(existing, "Passw0rd!x")).Succeeded);
        var service = scope.ServiceProvider.GetRequiredService<IExternalLoginService>();

        var result = await service.ResolveUserAsync(CreateLoginInfo("link-key", "link@example.test", true));

        Assert.Equal(existing.Id, result.User?.Id);
        Assert.True(await userManager.HasPasswordAsync(existing));
        Assert.Single(await userManager.GetLoginsAsync(existing));
    }

    [Fact]
    public async Task ResolveUserAsync_RemovesPassword_WhenLinkingUnconfirmedLocalAccount()
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Profile>>();
        var existing = new Profile { UserName = "squat@example.test", Email = "squat@example.test" };
        Assert.True((await userManager.CreateAsync(existing, "Passw0rd!x")).Succeeded);
        var service = scope.ServiceProvider.GetRequiredService<IExternalLoginService>();

        var result = await service.ResolveUserAsync(CreateLoginInfo("squat-key", "squat@example.test", true));

        Assert.True(result.Succeeded);
        Assert.False(await userManager.HasPasswordAsync(existing));
        Assert.True((await userManager.FindByIdAsync(existing.Id))?.EmailConfirmed);
    }
}
