using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Auth;

public interface IExternalLoginService
{
    Task<ExternalLoginResult> ResolveUserAsync(ExternalLoginInfo loginInfo);
}

public enum ExternalLoginFailure
{
    None,
    VerifiedEmailRequired,
    UserCreationFailed,
    LinkFailed,
    LockedOut
}

public sealed record ExternalLoginResult(Profile? User, ExternalLoginFailure Failure)
{
    public bool Succeeded => User is not null;

    public static ExternalLoginResult Success(Profile user) => new(user, ExternalLoginFailure.None);

    public static ExternalLoginResult Fail(ExternalLoginFailure failure) => new(null, failure);
}
