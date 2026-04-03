using FluentValidation;
using MediatR;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Behaviors;

/// <summary>
/// Executes FluentValidation validators before dispatching requests.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
            .SelectMany(result => result.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var resultType = typeof(TResponse);
        if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(OperationResult<,>))
        {
            var genericArgs = resultType.GetGenericArguments();
            var errorType = genericArgs[1];

            if (typeof(DomainError).IsAssignableFrom(errorType))
            {
                var first = failures[0];
                var badMethod = resultType.GetMethod("Bad");
                if (badMethod is not null)
                {
                    return (TResponse)badMethod.Invoke(null, [DomainError.Validation(first.ErrorCode ?? "validation.error", first.ErrorMessage)])!;
                }
            }
        }

        throw new ValidationException(failures);
    }
}
