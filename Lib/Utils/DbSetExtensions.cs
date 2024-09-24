using Lib.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lib.Utils;

public static class DbSetExtensions
{
    public static async Task<IEnumerable<(T Local, T Database)>> JoinAsync<T>(
        this DbSet<T> dbSet,
        IReadOnlyCollection<T> localModels
    ) where T : class, IDatabaseEntry
    {
        var query =
            from item in dbSet
            join id in localModels.Select(x => x.Id) on item.Id equals id
            select item;

        var results = await query.ToListAsync();

        return
            from local in localModels
            join result in results on local.Id equals result.Id
            select (local, result);
    }
}