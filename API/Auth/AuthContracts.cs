using System.ComponentModel.DataAnnotations;

namespace API.Auth;

public enum AuthMode
{
    Cookie,
    Jwt
}

public sealed class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;

    public AuthMode Mode { get; init; } = AuthMode.Cookie;
}

public sealed record UserResponse(string Id, string? Email, string? UserName);

public sealed record TokenResponse(string AccessToken, string TokenType, DateTimeOffset ExpiresAt);
