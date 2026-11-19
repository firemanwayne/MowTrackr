namespace MowTrackr.Core.Models;

/// <summary>
/// Framework-agnostic result of a service call. Bridged to HTTP by the API layer.
/// </summary>
/// <typeparam name="T">The success payload type.</typeparam>
public sealed class Result<T>
{
    private Result(ResultStatus status, T? value, string? error)
    {
        Status = status;

        Value = value;

        Error = error;
    }

    /// <summary>Gets the outcome category.</summary>
    public ResultStatus Status { get; }

    /// <summary>Gets the payload (set on success; may also carry detail on a conflict).</summary>
    public T? Value { get; }

    /// <summary>Gets the human-readable error, if any.</summary>
    public string? Error { get; }

    /// <summary>Gets a value indicating whether the call succeeded.</summary>
    public bool IsSuccess => Status == ResultStatus.Ok;

    /// <summary>Creates a success result.</summary>
    /// <param name="value">The payload.</param>
    /// <returns>The result.</returns>
    public static Result<T> Ok(T value) => new(ResultStatus.Ok, value, null);

    /// <summary>Creates a not-found result.</summary>
    /// <param name="error">The message.</param>
    /// <returns>The result.</returns>
    public static Result<T> NotFound(string error) => new(ResultStatus.NotFound, default, error);

    /// <summary>Creates an invalid-input result.</summary>
    /// <param name="error">The message.</param>
    /// <returns>The result.</returns>
    public static Result<T> Invalid(string error) => new(ResultStatus.Invalid, default, error);

    /// <summary>Creates a conflict result, optionally carrying detail.</summary>
    /// <param name="error">The message.</param>
    /// <param name="detail">Optional payload describing the conflict.</param>
    /// <returns>The result.</returns>
    public static Result<T> Conflict(string error, T? detail = default) => new(ResultStatus.Conflict, detail, error);

    /// <summary>Creates an unauthorized result.</summary>
    /// <param name="error">The message.</param>
    /// <returns>The result.</returns>
    public static Result<T> Unauthorized(string error) => new(ResultStatus.Unauthorized, default, error);

    /// <summary>Creates a forbidden result.</summary>
    /// <param name="error">The message.</param>
    /// <returns>The result.</returns>
    public static Result<T> Forbidden(string error) => new(ResultStatus.Forbidden, default, error);
}