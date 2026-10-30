using Core.Entities;

namespace API.Auth;

public interface IJwtTokenService
{
    AccessToken CreateToken(Profile user);
}

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
