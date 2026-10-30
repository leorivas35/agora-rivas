using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace API.Auth;

public static class GitHubEmailClaims
{
    private const string EmailsEndpoint = "https://api.github.com/user/emails";

    // Only a primary email GitHub reports as verified is trusted for account linking.
    public static async Task ApplyVerifiedPrimaryEmailAsync(OAuthCreatingTicketContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        RemoveClaims(context.Identity, ClaimTypes.Email);
        RemoveClaims(context.Identity, ExternalClaimTypes.EmailVerified);

        using var request = new HttpRequestMessage(HttpMethod.Get, EmailsEndpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

        using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        using var payload = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted));
        if (payload.RootElement.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var entry in payload.RootElement.EnumerateArray())
        {
            if (!IsTrue(entry, "primary") || !IsTrue(entry, "verified")
                || !entry.TryGetProperty("email", out var emailElement))
            {
                continue;
            }

            var address = emailElement.GetString();
            if (!string.IsNullOrWhiteSpace(address))
            {
                context.Identity?.AddClaim(new Claim(ClaimTypes.Email, address));
                context.Identity?.AddClaim(new Claim(ExternalClaimTypes.EmailVerified, "true"));
                return;
            }
        }
    }

    public static void Configure(GitHubAuthenticationOptions options)
    {
        options.Scope.Add("user:email");
        options.Events.OnCreatingTicket = ApplyVerifiedPrimaryEmailAsync;
    }

    private static bool IsTrue(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.True;

    private static void RemoveClaims(ClaimsIdentity? identity, string claimType)
    {
        if (identity is null)
        {
            return;
        }

        foreach (var claim in identity.FindAll(claimType).ToList())
        {
            identity.RemoveClaim(claim);
        }
    }
}
