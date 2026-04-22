public class Result<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
        => new(true, value, null);

    public static Result<T> Failure(string error)
        => new(false, default, error);

    public T GetValueOrThrow()
    {
        if (!IsSuccess)
            throw new InvalidOperationException(Error ?? "Result failed.");

        return Value!;
    }

    public TResult Match<TResult>(
        Func<T, TResult> onSuccess,
        Func<string, TResult> onFailure)
    {
        return IsSuccess
            ? onSuccess(Value!)
            : onFailure(Error!);
    }
}