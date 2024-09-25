using System.Collections;

namespace Lib.Utils;

public class Result<T> : IEnumerable<T>
{
    internal Result(T? value, Exception? error = null)
    {
        Value = value;
        ErrorValue = error;
    }

    private T? Value { get; }

    private Exception? ErrorValue { get; }

    public bool IsError => ErrorValue is not null;

    public bool IsOk => !IsError;

    public static implicit operator Result<T>(Exception error) => Result.Error<T>(error);

    public static implicit operator Result<T>(T value) => Result.Ok(value);

    public Result<TReturn> Map<TReturn>(Func<T, TReturn> map) =>
        IsOk
            ? Result.Ok(map(Value!))
            : Result.Error<TReturn>(ErrorValue!);

    public async Task<Result<TReturn>> MapAsync<TReturn>(Func<T, Task<TReturn>> map)
    {
        if (IsError)
        {
            return Result.Error<TReturn>(ErrorValue!);
        }

        var res = await map(Value!);

        return res;
    }

    public async Task<Result<TReturn>> FlatMapAsync<TReturn>(Func<T, Task<Result<TReturn>>> map)
    {
        if (IsError)
        {
            return Result.Error<TReturn>(ErrorValue!);
        }

        var res = await map(Value!);

        return res;
    }

    public Result<T> MapError(Func<Exception, Exception> map) =>
        IsOk
            ? this
            : Result.Error<T>(map(ErrorValue!));

    public Result<TReturn> FlatMap<TReturn>(Func<T, Result<TReturn>> map)
    {
        if (IsError) return Result.Error<TReturn>(ErrorValue!);
        var result = map(Value!);
        return result.IsError
            ? Result.Error<TReturn>(result.ErrorValue!)
            : Result.Ok<TReturn>(result.Value!);
    }

    public Result<T> IfOk(Action<T> action)
    {
        if (IsOk)
        {
            action(Value!);
        }

        return this;
    }

    public Result<T> IfError(Action<Exception> action)
    {
        if (IsError)
        {
            action(ErrorValue!);
        }

        return this;
    }

    public T Unwrap() =>
        IsOk
            ? Value!
            : throw new InvalidOperationException($"Cannot unwrap a result that is an error. Error: {ErrorValue}");

    public Exception UnwrapError() =>
        IsOk
            ? throw new InvalidOperationException($"Cannot unwrap an error that is a result. Result: {Value}")
            : ErrorValue!;

    public TReturn Match<TReturn>(Func<T, TReturn> okHandler, Func<Exception, TReturn> errHandler) =>
        IsOk
            ? okHandler(Value!)
            : errHandler(ErrorValue!);

    public Exception? GetError() => ErrorValue;

    public IEnumerator<T> GetEnumerator()
    {
        if (IsOk)
        {
            yield return Value!;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class Result
{
    public static Result<T> ToResult<T>(this T value) => value is Exception e ? Error<T>(e) : Ok(value);
    public static Result<T> Ok<T>(T value) => new(value);

    public static Result<T> Error<T>(Exception error) => new(default, error);

    public async static Task<Result<R>> Map<T,R>(this Task<Result<T>> task, Func<T, R> map)
    {
        var res = await task;
        return res.Map(map);
    }

    public async static Task<Result<R>> MapAsync<T,R>(this Task<Result<T>> task, Func<T, Task<R>> map)
    {
        var res = await task;
        return await res.MapAsync(map);
    }

    public async static Task<Result<R>> FlatMapAsync<T,R>(this Task<Result<T>> task, Func<T, Result<R>> map)
    {
        var res = await task;
        return res.FlatMap(map);
    }

    public async static Task<R> MatchAsync<T,R>(this Task<Result<T>> task, Func<T, Task<R>> okHandler, Func<Exception, Task<R>> errHandler)
    {
        var res = await task;
        return await res.Match(okHandler, errHandler);
    }

    public async static Task MatchAsync<T>(this Task<Result<T>> task, Func<T, Task> okHandler, Func<Exception, Task> errHandler)
    {
        var res = await task;
        await res.Match(okHandler, errHandler);
    }

    public static async Task<Exception?> ToExceptionAsync(this Task task)
    {
        try
        {
            await task;
        }
        catch (Exception e)
        {
            return e;
        }

        return null;
    }
}