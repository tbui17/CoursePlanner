using System.Reflection;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services;

public sealed class MultiDbContext<TDbContext, T>(
    IReadOnlyList<TDbContext> contexts,
    IReadOnlyList<(TDbContext, PropertyInfo)> pairs
)
    : IAsyncDisposable
    where T : class
    where TDbContext : DbContext
{
    private readonly IReadOnlyList<(TDbContext DbContext, PropertyInfo Property)> _pairs = pairs;

    public async Task<IList<TResult>> Query<TResult>(Func<IQueryable<T>, IQueryable<TResult>> query)
    {
        var results = await _pairs
            .Select(p => p.Property.GetValue(p.DbContext))
            .Cast<IQueryable<T>>()
            .Select(set => query(set).ToListAsync())
            .Thru(Task.WhenAll);

        return results.SelectMany(x => x).ToList();
    }

    public async ValueTask DisposeAsync()
    {
        await contexts
            .Select(x => x.DisposeAsync().AsTask())
            .Thru(Task.WhenAll);
    }
}

public abstract class MultiDbContextFactory<TDbContextFactory>(IDbContextFactory<TDbContextFactory> factory)
    where TDbContextFactory : DbContext
{
    public async Task<MultiDbContext<TDbContextFactory, TEntity>> CreateAsync<TEntity>() where TEntity : class
    {
        var properties = DbContextUtil.GetDbSets<TDbContextFactory, TEntity>().ToArray();
        var dbContexts = await properties.Length
            .Times(_ => factory.CreateDbContextAsync())
            .Thru(Task.WhenAll);
        var pairs = dbContexts.Zip(properties).ToList();
        var engine = new MultiDbContext<TDbContextFactory, TEntity>(dbContexts, pairs);
        return engine;
    }
}

public class MultiLocalDbContextFactory(ILocalDbCtxFactory factory) : MultiDbContextFactory<LocalDbCtx>(factory);