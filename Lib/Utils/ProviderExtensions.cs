using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Utils;

public static class ProviderExtensions
{
    public static Task<LocalDbCtx> GetLocalDbCtxAsync(this IServiceProvider provider)
    {
        var factory = provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>();
        return factory.CreateDbContextAsync();
    }
}