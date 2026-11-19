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

    /// <summary>Gets or sets the Radzen notification service.</summary>
    [Inject]
    protected NotificationService Notifications { get; set; } = default!;

    /// <summary>Shows a success toast.</summary>
    /// <param name="message">The message.</param>
    protected void NotifySuccess(string message)
    {
        Notifications.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = message, Duration = 3000 });
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