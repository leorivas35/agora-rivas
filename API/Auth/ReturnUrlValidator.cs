namespace API.Auth;

public sealed class ReturnUrlValidator(IConfiguration configuration)
{
    public const string ConfigurationKey = "Authentication:AllowedReturnOrigins";

    private readonly HashSet<string> allowedOrigins = new(
        configuration.GetSection(ConfigurationKey).Get<string[]>() ?? [],
        StringComparer.OrdinalIgnoreCase);

    public bool IsAllowed(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            return false;
        }

        if (IsLocalPath(returnUrl))
        {
            return true;
        }

        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            return false;
        }

        return allowedOrigins.Contains(uri.GetLeftPart(UriPartial.Authority).TrimEnd('/'));
    }

    private static bool IsLocalPath(string url)
    {
        if (url[0] != '/')
        {
            return false;
        }

        return url.Length == 1 || (url[1] != '/' && url[1] != '\\');
    }
}
