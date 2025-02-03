using SharedKernel;

namespace Web.Api.Infrastructure;

public static class CustomResults
{
    public static IResult Problem(Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("Cannot return a problem for a successful result.");
        }

        var error = result.Error;

        return Results.Problem(
            title: GetTitle(error),
            detail: GetDetail(error),
            type: GetErrorTypeUrl(error.Type),
            statusCode: GetStatusCode(error.Type),
            extensions: GetValidationErrors(result));
    }

    private static string GetTitle(Error? error)
    {
        return error?.Code ?? "Server failure";
    }

    private static string GetDetail(Error? error)
    {
        return error?.Description ?? "An unexpected error occurred";
    }

    private static string GetErrorTypeUrl(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
    }

    private static int GetStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Problem => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static Dictionary<string, object?>? GetValidationErrors(Result result)
    {
        return result.Error is ValidationError validationError
            ? new Dictionary<string, object?> { { "errors", validationError.Errors } }
            : null;
    }
}
