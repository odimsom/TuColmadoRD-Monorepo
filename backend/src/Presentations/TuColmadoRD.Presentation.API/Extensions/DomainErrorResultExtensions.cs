using Microsoft.AspNetCore.Http.HttpResults;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Presentation.API.Extensions;

/// <summary>
/// Maps domain errors to HTTP results.
/// </summary>
public static class DomainErrorResultExtensions
{
    /// <summary>
    /// Maps a domain error to an API result.
    /// </summary>
    public static IResult MapDomainError(this DomainError error)
    {
        if (error.Code.Contains("not_found", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.NotFound(new { error = error.Code, message = error.Message, statusCode = StatusCodes.Status404NotFound });
        }

        if (error.Code.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("required", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("unknown", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("too_long", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("below_cost", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("out_of_range", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest(new { error = error.Code, message = error.Message, statusCode = StatusCodes.Status400BadRequest });
        }

        if (error.Code.Contains("insufficient", StringComparison.OrdinalIgnoreCase) ||
            error.Code.Contains("business", StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.UnprocessableEntity(new { error = error.Code, message = error.Message, statusCode = StatusCodes.Status422UnprocessableEntity });
        }

        return TypedResults.Problem(error.Message, statusCode: StatusCodes.Status500InternalServerError, title: error.Code);
    }
}
