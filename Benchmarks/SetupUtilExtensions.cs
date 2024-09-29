using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

public static class SetupUtilExtensions
{
    public static async Task<LocalDbCtx> GetDb(this ServiceProvider provider)
    {
        return await provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>().CreateDbContextAsync();
    }
}