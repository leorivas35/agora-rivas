using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using API.Auth;

namespace API.Tests;

public class AuthEndpointsTests : IClassFixture<AuthApiFactory>
{
    private const string Password = "Passw0rd!x";

    private readonly AuthApiFactory factory;

    public AuthEndpointsTests(AuthApiFactory factory)
    {
        this.factory = factory;
    }

    private static async Task RegisterAsync(HttpClient client, string email)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new { email, password = Password });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Me_ReturnsUnauthorized_WithoutCredentials()
    {
        using var client = factory.CreateSecureClient();

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task JwtLogin_AllowsAccessToMe()
    {
        using var client = factory.CreateSecureClient();
        await RegisterAsync(client, "jwt@example.test");

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "jwt@example.test", password = Password, mode = "Jwt" });
        var token = await login.Content.ReadFromJsonAsync<TokenResponse>();

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.NotNull(token);

        using var bearerClient = factory.CreateSecureClient();
        bearerClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        var me = await bearerClient.GetFromJsonAsync<UserResponse>("/api/auth/me");

        Assert.Equal("jwt@example.test", me?.Email);
    }

    [Fact]
    public async Task Me_RejectsTamperedJwt()
    {
        using var client = factory.CreateSecureClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not.a.jwt");

        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CookieLogin_AllowsAccessToMe_UntilLogout()
    {
        using var client = factory.CreateSecureClient();
        await RegisterAsync(client, "cookie@example.test");

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email = "cookie@example.test", password = Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/auth/me")).StatusCode);

        await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_ForWrongPassword()
    {
        using var client = factory.CreateSecureClient();
        await RegisterAsync(client, "wrong@example.test");

        var response = await client.PostAsJsonAsync("/api/auth/login", new { email = "wrong@example.test", password = "Wrong-pass1" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_RejectsWeakPassword()
    {
        using var client = factory.CreateSecureClient();

        var response = await client.PostAsJsonAsync("/api/auth/register", new { email = "weak@example.test", password = "abc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ExternalLogin_RedirectsToConfiguredProvider()
    {
        using var client = factory.CreateSecureClient();

        var response = await client.GetAsync($"/api/auth/external/google?returnUrl={Uri.EscapeDataString(AuthApiFactory.AllowedOrigin + "/done")}&mode=Jwt");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var location = response.Headers.Location?.ToString() ?? string.Empty;
        Assert.StartsWith("https://accounts.google.com/", location);
        Assert.Contains("redirect_uri=", location);
    }

    [Fact]
    public async Task ExternalLogin_ReturnsNotFound_ForUnconfiguredOrUnknownProvider()
    {
        using var client = factory.CreateSecureClient();

        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/auth/external/github")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/api/auth/external/facebook")).StatusCode);
    }

    [Fact]
    public async Task ExternalLogin_RejectsUntrustedReturnUrl()
    {
        using var client = factory.CreateSecureClient();

        var response = await client.GetAsync("/api/auth/external/google?returnUrl=https%3A%2F%2Fevil.example%2F");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
