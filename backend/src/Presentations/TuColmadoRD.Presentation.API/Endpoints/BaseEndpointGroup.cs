using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;
using TuColmadoRD.Presentation.API.Extensions;

namespace TuColmadoRD.Presentation.API.Endpoints;

/// <summary>
/// Base class for endpoint groups providing common functionality for result handling,
/// error mapping, and API response patterns.
/// </summary>
public abstract class BaseEndpointGroup
{
    /// <summary>
    /// Maps any error result to an appropriate HTTP response.
    /// </summary>
    protected IResult HandleError<TError>(TError error) where TError : class
    {
        if (error is DomainError domainError)
        {
            return domainError.MapDomainError();
        }

        return TypedResults.BadRequest(new { message = error?.ToString() ?? "Unknown error" });
    }

    /// <summary>
    /// Safely extracts and uses a result value, or returns an error response.
    /// </summary>
    protected bool TryGetResultAndReturn<TResult, TError>(
        in OperationResult<TResult, TError> operationResult,
        out TResult? value,
        out IResult? errorResponse)
        where TError : class
    {
        if (operationResult.TryGetResult(out value))
        {
            errorResponse = null;
            return true;
        }

        errorResponse = HandleError(operationResult.Error);
        return false;
    }

    /// <summary>
    /// Configures a route group with standard settings (authorization, tags, API versioning).
    /// </summary>
    protected RouteGroupBuilder ConfigureGroup(
        RouteGroupBuilder group,
        string tag,
        bool requiresAuthorization = true)
    {
        group.WithTags(tag);

        if (requiresAuthorization)
        {
            group.RequireAuthorization();
        }

        return group;
    }
}
