namespace MowTrackr.Client.Services;

/// <summary>
/// Default <see cref="ISessionExpiry"/>.
/// </summary>
internal sealed class SessionExpiry : ISessionExpiry
{
    private bool _notice;

    /// <inheritdoc />
    public event Action? Expired;

    /// <inheritdoc />
    public void FlagExpired()
    {
        _notice = true;
    }

    /// <inheritdoc />
    public void SignalExpired()
    {
        _notice = true;

        Expired?.Invoke();
    }

    /// <inheritdoc />
    public bool ConsumeNotice()
    {
        var notice = _notice;

        _notice = false;

        return notice;
    }
}