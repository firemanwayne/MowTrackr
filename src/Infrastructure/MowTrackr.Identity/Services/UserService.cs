using System.Text.RegularExpressions;
using MowTrackr.Core.Constants;
using MowTrackr.Identity.Extensions;
using MowTrackr.Identity.Models;

namespace MowTrackr.Identity.Services;

/// <summary>
/// Default <see cref="IUserService"/>.
/// </summary>
/// <param name="userManager">The user manager.</param>
/// <param name="sms">The SMS sender (phone verification codes).</param>
/// <param name="logger">The logger.</param>
internal sealed partial class UserService(UserManager<ApplicationUser> userManager, ISmsService sms, ILogger<UserService> logger) : IUserService
{
    private const string InvalidPhoneMessage = "Enter the number in international format, for example +15551234567.";

    private const string BadPhoneCodeMessage = "That code is not right or has expired. Request a new one.";
    /// <inheritdoc />
    public async Task<Result<UserResponse>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<UserResponse>>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users.OrderBy(u => u.DisplayName).ToListAsync(cancellationToken).ConfigureAwait(false);

        var responses = new List<UserResponse>(users.Count);

        foreach (var user in users)
        {
            responses.Add(await ToResponseAsync(user).ConfigureAwait(false));
        }

        return Result<IReadOnlyList<UserResponse>>.Ok(responses);
    }

    /// <inheritdoc />
    public async Task<Result<UserResponse>> UpdateProfileAsync(Guid id, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        if (!AppLanguages.IsSupported(request.Language))
        {
            return Result<UserResponse>.Invalid($"Unsupported language '{request.Language}'.");
        }

        if (!AppThemes.IsSupported(request.Theme))
        {
            return Result<UserResponse>.Invalid($"Unsupported theme '{request.Theme}'.");
        }

        user.DisplayName = request.DisplayName.Trim();

        user.Language = request.Language.ToLowerInvariant();

        user.Theme = AppThemes.All.First(t => string.Equals(t, request.Theme, StringComparison.OrdinalIgnoreCase));

        var updated = await userManager.UpdateAsync(user).ConfigureAwait(false);

        if (!updated.Succeeded)
        {
            return Result<UserResponse>.Invalid(string.Join(" ", updated.Errors.Select(e => e.Description)));
        }

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<StartPhoneVerificationResponse>> StartPhoneVerificationAsync(Guid id, StartPhoneVerificationRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<StartPhoneVerificationResponse>.NotFound("User not found.");
        }

        var phone = NormalizePhone(request.PhoneNumber);

        if (phone is null)
        {
            return Result<StartPhoneVerificationResponse>.Invalid(InvalidPhoneMessage);
        }

        var code = await userManager.GenerateChangePhoneNumberTokenAsync(user, phone).ConfigureAwait(false);

        var sent = false;

        try
        {
            sent = await sms.SendVerificationCodeAsync(new SmsVerification(phone, code, user.Language), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to text a verification code for user {UserId}", user.Id);
        }

        return Result<StartPhoneVerificationResponse>.Ok(new StartPhoneVerificationResponse(sent));
    }

    /// <inheritdoc />
    public async Task<Result<UserResponse>> VerifyPhoneAsync(Guid id, VerifyPhoneRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var phone = NormalizePhone(request.PhoneNumber);

        if (phone is null)
        {
            return Result<UserResponse>.Invalid(InvalidPhoneMessage);
        }

        var changed = await userManager.ChangePhoneNumberAsync(user, phone, request.Code.Trim()).ConfigureAwait(false);

        if (!changed.Succeeded)
        {
            return Result<UserResponse>.Invalid(BadPhoneCodeMessage);
        }

        logger.LogInformation("Phone number verified for user {UserId}", user.Id);

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <inheritdoc />
    public async Task<Result<UserResponse>> SetRolesAsync(Guid id, SetRolesRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString()).ConfigureAwait(false);

        if (user is null)
        {
            return Result<UserResponse>.NotFound("User not found.");
        }

        var wanted = request.Roles.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var unknown = wanted.Where(r => !AppRoles.All.Contains(r, StringComparer.OrdinalIgnoreCase)).ToList();

        if (unknown.Count > 0)
        {
            return Result<UserResponse>.Invalid($"Unknown role(s): {string.Join(", ", unknown)}.");
        }

        var current = await userManager.GetRolesAsync(user).ConfigureAwait(false);

        var removingAdmin = current.Contains(AppRoles.Admin) && !wanted.Contains(AppRoles.Admin, StringComparer.OrdinalIgnoreCase);

        if (removingAdmin)
        {
            var admins = await userManager.GetUsersInRoleAsync(AppRoles.Admin).ConfigureAwait(false);

            if (admins.Count <= 1)
            {
                return Result<UserResponse>.Conflict("At least one Admin must remain.");
            }
        }

        await userManager.RemoveFromRolesAsync(user, current.Except(wanted, StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

        await userManager.AddToRolesAsync(user, wanted.Except(current, StringComparer.OrdinalIgnoreCase)).ConfigureAwait(false);

        logger.LogInformation("Set roles for user {UserId} to {Roles}", user.Id, wanted);

        return Result<UserResponse>.Ok(await ToResponseAsync(user).ConfigureAwait(false));
    }

    /// <summary>Strips formatting from a phone number and checks it is in international (E.164) format.</summary>
    /// <param name="input">What the user typed.</param>
    /// <returns>The number such as +15551234567, or <see langword="null"/> if it is not valid.</returns>
    private static string? NormalizePhone(string input)
    {
        var cleaned = new string(input.Where(c => char.IsDigit(c) || c == '+').ToArray());

        return PhoneNumberPattern().IsMatch(cleaned) ? cleaned : null;
    }

    [GeneratedRegex(@"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneNumberPattern();

    /// <summary>Maps a user to its response, loading roles.</summary>
    /// <param name="user">The user.</param>
    /// <returns>The response.</returns>
    private async Task<UserResponse> ToResponseAsync(ApplicationUser user)
    {
        var roles = (await userManager.GetRolesAsync(user).ConfigureAwait(false)).ToList();

        return user.ToResponse(roles);
    }
}