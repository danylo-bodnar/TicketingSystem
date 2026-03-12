namespace Ticketing.Application.Common;

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
        => new Result<T>(true, value, null);

    public static Result<T> Failure(string error)
        => new Result<T>(false, default, error);

    public T GetValueOrThrow()
    => Value ?? throw new InvalidOperationException("Result has no value.");
}