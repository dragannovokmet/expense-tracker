using ExpenseTracker.Core.Users;

using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Infrastructure.Identity;

/// <summary>
/// Makes Identity use the password rule from Core (US-AUTH-01/AC5–AC7), so the rule exists in one place.
/// Identity's own password options are switched off in <see cref="DependencyInjection"/>.
/// </summary>
public sealed class CorePasswordValidator : IPasswordValidator<AppUser>
{
    public Task<IdentityResult> ValidateAsync(UserManager<AppUser> manager, AppUser user, string? password)
    {
        var error = PasswordRules.Validate(password);

        var result = error is null
            ? IdentityResult.Success
            : IdentityResult.Failed(new IdentityError { Code = "PasswordRules", Description = error });

        return Task.FromResult(result);
    }
}
