namespace TuColmadoRD.Core.Domain.Base.Result;

public static class OperationResultExtensions
{
    public static async Task<OperationResult<TNew, TError>> BindAsync<T, TNew, TError>(
        this Task<OperationResult<T, TError>> resultTask,
        Func<T?, Task<OperationResult<TNew, TError>>> binder)
    {
        var result = await resultTask;
        return await result.BindAsync(binder);
    }

    public static async Task<OperationResult<TNew, TError>> MapAsync<T, TNew, TError>(
        this Task<OperationResult<T, TError>> resultTask,
        Func<T?, Task<TNew>> mapper)
    {
        var result = await resultTask;
        return await result.MapAsync(mapper);
    }

    public static async Task<TOut> MatchAsync<T, TError, TOut>(
        this Task<OperationResult<T, TError>> resultTask,
        Func<T?, TOut> onGood,
        Func<TError?, TOut> onBad)
    {
        var result = await resultTask;
        return result.Match(onGood, onBad);
    }
}