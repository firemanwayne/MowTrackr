namespace MowTrackr.Api.Extensions;

/// <summary>
/// Bridges the framework-agnostic <see cref="Result{T}"/> to ASP.NET Core responses in one place.
/// </summary>
public static class ResultExtensions
{
    /// <summary>Converts a result to an <see cref="IActionResult"/>; success is 200 with the value.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The HTTP response.</returns>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return result.ToFailure();
    }

    /// <summary>Converts a result to an <see cref="IActionResult"/>; success is 204 with no body.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="result">The result.</param>
    /// <returns>The HTTP response.</returns>
    public static IActionResult ToNoContentResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return result.ToFailure();
    }

    /// <summary>Maps a failed result to its HTTP status. A conflict carrying detail returns the detail as the body.</summary>
    /// <typeparam name="T">The payload type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <returns>The HTTP response.</returns>
    private static IActionResult ToFailure<T>(this Result<T> result)
    {
        var (status, title) = result.Status switch
        {
            ResultStatus.NotFound => (StatusCodes.Status404NotFound, "Not found"),
            ResultStatus.Invalid => (StatusCodes.Status400BadRequest, "Invalid request"),
            ResultStatus.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ResultStatus.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ResultStatus.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => (StatusCodes.Status500InternalServerError, "Error"),
        };

        if (result.Status == ResultStatus.Conflict && result.Value is not null)
        {
            return new ObjectResult(result.Value) { StatusCode = status };
        }

        var problem = new ProblemDetails { Status = status, Title = title, Detail = result.Error };

        return new ObjectResult(problem) { StatusCode = status };
    }
}