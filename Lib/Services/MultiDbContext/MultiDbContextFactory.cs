using Lib.Utils;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.MultiDbContext;

public abstract class MultiDbContextFactory<TDbContextFactory>(IDbContextFactory<TDbContextFactory> factory)
    where TDbContextFactory : DbContext
{
    public async Task<MultiDbContext<TDbContextFactory, TEntity>> CreateAsync<TEntity>() where TEntity : class
    {
        var properties = DbContextUtil.GetDbSets<TDbContextFactory, TEntity>().ToArray();
        var dbContexts = await properties.Length
            .Times(_ => factory.CreateDbContextAsync())
            .Thru(Task.WhenAll);
        var dbSets = dbContexts
            .Zip(properties)
            .Select(x => x.Second.GetValue(x.First))
            .Cast<IQueryable<TEntity>>()
            .ToList();
        return new MultiDbContext<TDbContextFactory, TEntity>(dbContexts, dbSets);
    }
}