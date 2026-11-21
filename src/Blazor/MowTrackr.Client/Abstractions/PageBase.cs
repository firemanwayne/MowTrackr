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
    private const int CooldownMilliseconds = 400;

    /// <summary>How long an error message stays up. Long enough to read, and a click closes it sooner.</summary>
    private const int ErrorMilliseconds = 60_000;

    /// <summary>How long a warning stays up.</summary>
    private const int WarningMilliseconds = 20_000;

    private readonly CancellationTokenSource _cancellation = new();

    private readonly HashSet<string> _running = [];

    /// <summary>Initializes the page, taking its cancellation token once so it stays readable after the page is disposed.</summary>
    protected PageBase()
    {
        Cancellation = _cancellation.Token;
    }

    /// <summary>
    /// Gets a token that is cancelled when the component is disposed. It is a copy taken up front: reading it from the
    /// (disposed) source after the page has gone, as a button's cooldown or a late response does, would throw.
    /// </summary>
    protected CancellationToken Cancellation { get; }

    /// <summary>Gets or sets the string localizer (English text is the key; Spanish comes from the Localization library).</summary>
    [Inject]
    protected IStringLocalizer<SharedResource> L { get; set; } = default!;

    /// <summary>Gets or sets the Radzen notification service.</summary>
    [Inject]
    protected NotificationService Notifications { get; set; } = default!;

    /// <summary>
    /// Runs an action at most once at a time. While it runs, and for a short cooldown afterwards, further calls with the
    /// same key are ignored, so double-clicks, key repeats and impatient tapping cannot send the same request twice.
    /// </summary>
    /// <param name="key">Names the action; calls with different keys do not block each other.</param>
    /// <param name="action">The action to run.</param>
    /// <returns>A task.</returns>
    protected async Task RunOnceAsync(string key, Func<Task> action)
    {
        if (!_running.Add(key))
        {
            return;
        }

        try
        {
            await action();
        }
        finally
        {
            await ReleaseAfterCooldownAsync(key);
        }
    }

    /// <summary>Gets a value indicating whether an action started with <see cref="RunOnceAsync"/> is still busy or cooling down.</summary>
    /// <param name="key">The action's key.</param>
    /// <returns><see langword="true"/> while the action is guarded (use it for a button's busy or disabled state).</returns>
    protected bool IsRunning(string key)
    {
        return _running.Contains(key);
    }

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
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = message, Duration = WarningMilliseconds, CloseOnClick = true });
    }

    /// <summary>Shows an error toast.</summary>
    /// <param name="message">The message.</param>
    protected void NotifyError(string message)
    {
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = message, Duration = ErrorMilliseconds, CloseOnClick = true });
    }

    /// <summary>Waits out the cooldown, then lets the action run again.</summary>
    /// <param name="key">The action's key.</param>
    /// <returns>A task.</returns>
    private async Task ReleaseAfterCooldownAsync(string key)
    {
        try
        {
            await Task.Delay(CooldownMilliseconds, Cancellation);
        }
        catch (OperationCanceledException)
        {
            // The component went away; nothing left to update.
        }

        _running.Remove(key);

        if (!Cancellation.IsCancellationRequested)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cancellation.Cancel();

        _cancellation.Dispose();

        GC.SuppressFinalize(this);
    }
}