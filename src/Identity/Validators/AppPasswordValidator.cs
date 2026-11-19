using MowTrackr.Core.Constants;
using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Validators;

/// <summary>
/// Enforces <see cref="PasswordPolicy"/> (the same rules the client shows as hints) and reports each broken rule.
/// </summary>
internal sealed class AppPasswordValidator : IPasswordValidator<ApplicationUser>
{
    /// <inheritdoc />
    public Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        var failures = PasswordPolicy.Failures(password, [user.Email, user.UserName, user.DisplayName]);

        if (failures.Count == 0)
        {
            return Task.FromResult(IdentityResult.Success);
        }

        var errors = failures.Select(rule => new IdentityError { Code = $"Password{rule}", Description = Describe(rule) });

        return Task.FromResult(IdentityResult.Failed(errors.ToArray()));
    }

    /// <summary>Gets the message for a broken rule.</summary>
    /// <param name="rule">The rule.</param>
    /// <returns>The message.</returns>
    private static string Describe(PasswordRule rule)
    {
        return rule switch
        {
            PasswordRule.MinimumLength => $"The password must be at least {PasswordPolicy.MinLength} characters.",
            PasswordRule.MaximumLength => $"The password must be at most {PasswordPolicy.MaxLength} characters.",
            PasswordRule.Uppercase => "The password must contain an upper-case letter.",
            PasswordRule.Lowercase => "The password must contain a lower-case letter.",
            PasswordRule.Digit => "The password must contain a digit.",
            PasswordRule.Symbol => "The password must contain a symbol, such as ! or #.",
            PasswordRule.NoPersonalInfo => "The password must not contain your name or email address.",
            PasswordRule.NotCommon => "That password is too common or easy to guess.",
            _ => "The password is not acceptable.",
        };
    }
}