namespace API.Auth;

public sealed class ExternalProviderOptions
{
    public string? ClientId { get; init; }

    public string? ClientSecret { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId) && !string.IsNullOrWhiteSpace(ClientSecret);

    public bool IsPartiallyConfigured =>
        !IsConfigured && (!string.IsNullOrWhiteSpace(ClientId) || !string.IsNullOrWhiteSpace(ClientSecret));
}
