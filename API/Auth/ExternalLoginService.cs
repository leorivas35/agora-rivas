using System.Security.Claims;
using Core.Constants;
using Core.Entities;
using Microsoft.AspNetCore.Identity;

namespace API.Auth;

public sealed class ExternalLoginService(UserManager<Profile> userManager) : IExternalLoginService
{
    public async Task<ExternalLoginResult> ResolveUserAsync(ExternalLoginInfo loginInfo)
    {
        ArgumentNullException.ThrowIfNull(loginInfo);

        var existingLoginUser = await userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
        if (existingLoginUser is not null)
        {
            return await userManager.IsLockedOutAsync(existingLoginUser)
                ? ExternalLoginResult.Fail(ExternalLoginFailure.LockedOut)
                : ExternalLoginResult.Success(existingLoginUser);
        }

        var email = GetVerifiedEmail(loginInfo.Principal);
        if (email is null)
        {
            return ExternalLoginResult.Fail(ExternalLoginFailure.VerifiedEmailRequired);
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new Profile
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                AboutMe = default(Document)!,
                ProfilePictureUrl = string.Empty
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return ExternalLoginResult.Fail(ExternalLoginFailure.UserCreationFailed);
            }
        }
        else
        {
            if (await userManager.IsLockedOutAsync(user))
            {
                return ExternalLoginResult.Fail(ExternalLoginFailure.LockedOut);
            }

            if (!await ConfirmLinkedAccountAsync(user))
            {
                return ExternalLoginResult.Fail(ExternalLoginFailure.LinkFailed);
            }
        }

        var linkResult = await userManager.AddLoginAsync(user, loginInfo);
        return linkResult.Succeeded
            ? ExternalLoginResult.Success(user)
            : ExternalLoginResult.Fail(ExternalLoginFailure.LinkFailed);
    }

    // A local account created with an unconfirmed email could be pre-registered by an attacker who knows
    // the password. Remove that password and confirm the email, since the provider proved ownership.
    private async Task<bool> ConfirmLinkedAccountAsync(Profile user)
    {
        if (user.EmailConfirmed)
        {
            return true;
        }

        if (await userManager.HasPasswordAsync(user)
            && !(await userManager.RemovePasswordAsync(user)).Succeeded)
        {
            return false;
        }

        user.EmailConfirmed = true;
        return (await userManager.UpdateAsync(user)).Succeeded;
    }

    private static string? GetVerifiedEmail(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var isVerified = string.Equals(
            principal.FindFirstValue(ExternalClaimTypes.EmailVerified),
            "true",
            StringComparison.OrdinalIgnoreCase);

        return isVerified && !string.IsNullOrWhiteSpace(email) ? email : null;
    }
}
