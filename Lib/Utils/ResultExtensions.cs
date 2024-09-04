using FluentResults;

namespace Lib.Utils;

public static class ResultExtensions
{
    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Task<Result<T>> task,
        Func<T, Task<Result<TResult>>> mapper)
    {
        var res = await task;
        if (res.IsFailed)
        {
            return res.ToResult<TResult>();
        }

        return await mapper(res.Value);
    }

    public static async Task<Result<TResult>> BindAsync<T, TResult>(this Result<T> result,
        Func<T, Task<Result<TResult>>> mapper)
    {
        if (result.IsFailed)
        {
            return result.ToResult<TResult>();
        }

        return await mapper(result.Value);
    }

    public static async Task<Result<TResult>> MapAsync<T, TResult>(this Result<T> result,
        Func<T, Task<TResult>> mapper)
    {
        if (result.IsFailed)
        {
            return result.ToResult<TResult>();
        }

        return await mapper(result.Value);
    }

    public static string ToErrorString<T>(this Result<T> result)
    {
        return result.Errors.Select(x => x.Message).StringJoin(Environment.NewLine);
    }
}