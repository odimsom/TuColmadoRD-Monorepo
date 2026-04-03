using System;

namespace TuColmadoRD.Core.Domain.Base.Result
{
    public class OperationResult<TResult, TError>
    {
        private readonly TResult? _result;
        private readonly TError? _error;

        public bool IsGood { get; }

        public bool TryGetResult(out TResult? result)
        {
            result = _result;
            return IsGood;
        }

        public bool TryGetError(out TError? error)
        {
            error = _error;
            return !IsGood;
        }

        public TResult Result => IsGood
            ? _result!
            : throw new InvalidOperationException($"Operation failed. Error: {_error}");

        public TError Error => !IsGood
            ? _error!
            : throw new InvalidOperationException("Cannot access error of a successful operation.");

        private OperationResult(bool isGood, TResult? result, TError? error)
        {
            IsGood = isGood;
            _result = result;
            _error = error;
        }

        public static OperationResult<TResult, TError> Good(TResult? result) =>
            new(true, result, default);

        public static OperationResult<TResult, TError> Bad(TError? error) =>
            new(false, default, error);

        public TOut Match<TOut>(
            Func<TResult?, TOut> onGood,
            Func<TError?, TOut> onBad) =>
            IsGood ? onGood(_result) : onBad(_error);

        public void Match(
            Action<TResult?> onGood,
            Action<TError?> onBad)
        {
            if (IsGood) onGood(_result);
            else onBad(_error);
        }

        public OperationResult<TNewResult, TError> Map<TNewResult>(
            Func<TResult?, TNewResult> mapper) =>
            IsGood
                ? OperationResult<TNewResult, TError>.Good(mapper(_result))
                : OperationResult<TNewResult, TError>.Bad(_error);

        public OperationResult<TNewResult, TError> Bind<TNewResult>(
            Func<TResult?, OperationResult<TNewResult, TError>> binder) =>
            IsGood
                ? binder(_result)
                : OperationResult<TNewResult, TError>.Bad(_error);

        public async Task<OperationResult<TNewResult, TError>> MapAsync<TNewResult>(
            Func<TResult?, Task<TNewResult>> mapper) =>
            IsGood
                ? OperationResult<TNewResult, TError>.Good(await mapper(_result))
                : OperationResult<TNewResult, TError>.Bad(_error);

        public async Task<OperationResult<TNewResult, TError>> BindAsync<TNewResult>(
            Func<TResult?, Task<OperationResult<TNewResult, TError>>> binder) =>
            IsGood
                ? await binder(_result)
                : OperationResult<TNewResult, TError>.Bad(_error);

        public OperationResult<TResult, TError> Tap(Action<TResult?> onGood)
        {
            if (IsGood) onGood(_result);
            return this;
        }
    }
}