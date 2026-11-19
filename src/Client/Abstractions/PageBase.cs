using MowTrackr.Core.Enums;
using Microsoft.Extensions.Localization;
using MowTrackr.Localization.Resources;
using Radzen;

namespace MowTrackr.Client.Abstractions;

/// <summary>
/// Base for pages and dialogs: a cancellation token tied to the component's lifetime plus notification helpers.
/// </summary>
public abstract class PageBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _cancellation = new();

    /// <summary>Gets a token that is cancelled when the component is disposed.</summary>
    protected CancellationToken Cancellation => _cancellation.Token;

    /// <summary>Gets or sets the string localizer (English text is the key; Spanish comes from the Localization library).</summary>
    [Inject]
    protected IStringLocalizer<SharedResource> L { get; set; } = default!;

    /// <summary>Gets or sets the Radzen notification service.</summary>
    [Inject]
    protected NotificationService Notifications { get; set; } = default!;

    /// <summary>Gets the localized display text of a work order status.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The text.</returns>
    protected string StatusText(WorkOrderStatus status)
    {
        return status switch
        {
            WorkOrderStatus.Scheduled => L["Scheduled"],
            WorkOrderStatus.InProgress => L["In progress"],
            WorkOrderStatus.Completed => L["Completed"],
            WorkOrderStatus.Cancelled => L["Cancelled"],
            _ => status.ToString(),
        };
    }
    /// <summary>Gets the chip colour for a work order status.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The badge style.</returns>
    protected static BadgeStyle StatusStyle(WorkOrderStatus status)
    {
        return status switch
        {
            WorkOrderStatus.Scheduled => BadgeStyle.Info,
            WorkOrderStatus.InProgress => BadgeStyle.Warning,
            WorkOrderStatus.Completed => BadgeStyle.Success,
            _ => BadgeStyle.Light,
        };
    }

    /// <summary>Gets the localized display text of an invitation status.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The text.</returns>
    protected string InvitationStatusText(InvitationStatus status)
    {
        return status switch
        {
            InvitationStatus.Pending => L["Pending"],
            InvitationStatus.Accepted => L["Accepted"],
            InvitationStatus.Revoked => L["Revoked"],
            InvitationStatus.Expired => L["Expired"],
            _ => status.ToString(),
        };
    }

    /// <summary>Gets the chip colour for an invitation status.</summary>
    /// <param name="status">The status.</param>
    /// <returns>The badge style.</returns>
    protected static BadgeStyle InvitationStatusStyle(InvitationStatus status)
    {
        return status switch
        {
            InvitationStatus.Pending => BadgeStyle.Info,
            InvitationStatus.Accepted => BadgeStyle.Success,
            InvitationStatus.Expired => BadgeStyle.Warning,
            _ => BadgeStyle.Light,
        };
    }

    /// <summary>Shows a success toast.</summary>
    /// <param name="message">The message.</param>
    protected void NotifySuccess(string message)
    {
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = message, Duration = 3000 });
    }

    /// <summary>Shows a warning toast.</summary>
    /// <param name="message">The message.</param>
    protected void NotifyWarning(string message)
    {
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = message, Duration = 8000 });
    }

    /// <summary>Shows an error toast.</summary>
    /// <param name="message">The message.</param>
    protected void NotifyError(string message)
    {
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = message, Duration = 6000 });
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cancellation.Cancel();

        _cancellation.Dispose();

        GC.SuppressFinalize(this);
    }
}