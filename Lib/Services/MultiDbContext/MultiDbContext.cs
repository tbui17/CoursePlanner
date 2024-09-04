using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.MultiDbContext;

public sealed class MultiDbContext<TDbContext, T>(
    IReadOnlyList<TDbContext> contexts,
    IReadOnlyList<IQueryable<T>> dbSets
)
    : IAsyncDisposable
    where T : class
    where TDbContext : DbContext
{

    public async Task<IList<TResult>> Query<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        var results = await dbSets
            .Select(set => query(set).ToListAsync())
            .Thru(Task.WhenAll);

        return results.SelectMany(x => x).ToList();
    }

    public async Task<IList<TResult>> Query<TResult>(Func<IQueryable<T>, Task<TResult>> query)
    {
        return await dbSets
            .Select(query)
            .Thru(Task.WhenAll);
    }

    public async Task<IList<TResult>> Query<TResult>(Func<IQueryable<T>, TDbContext, Task<TResult>> query) =>
        await dbSets
            .Zip(contexts)
            .Select(x => query(x.First, x.Second))
            .Thru(Task.WhenAll);

    public async ValueTask DisposeAsync()
    {
        await contexts
            .Select(x => x.DisposeAsync().AsTask())
            .Thru(Task.WhenAll);
    }
}