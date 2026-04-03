using MediatR;
using TuColmadoRD.Core.Application.Interfaces.Security;
using TuColmadoRD.Core.Domain.Base.Result;
using TuColmadoRD.Core.Domain.Errors;
using TuColmadoRD.Core.Domain.ValueObjects.Base;

namespace TuColmadoRD.Core.Application.Behaviors;

public interface ICommandMarker { }

public class ClockAdvancePipelineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ITimeGuard _timeGuard;

    public ClockAdvancePipelineBehavior(ITimeGuard timeGuard)
    {
        _timeGuard = timeGuard;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ICommandMarker)
        {
            return await next();
        }

        var advanceResult = await _timeGuard.AdvanceTimeAsync(DateTime.UtcNow);
        
        if (!advanceResult.IsGood)
        {
            advanceResult.TryGetError(out var timeError);
            
            var resultType = typeof(TResponse);
            if (resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(OperationResult<,>))
            {
                var genericArgs = resultType.GetGenericArguments();
                var errorType = genericArgs[1];
                
                if (typeof(DomainError).IsAssignableFrom(errorType))
                {
                    var badMethod = resultType.GetMethod("Bad");
                    if (badMethod != null)
                    {
                        return (TResponse)badMethod.Invoke(null, new object[] { timeError! })!;
                    }
                }
            }
            
            throw new InvalidOperationException($"LKT anti-tamper detected, but could not propagate error cleanly to {typeof(TResponse).Name}");
        }

        return await next();
    }
}
