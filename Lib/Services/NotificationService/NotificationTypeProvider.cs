using Humanizer;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.Services.NotificationService;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class NotificationTypeProvider(IMemoryCache cache, ILogger<NotificationTypeProvider> logger)
{
    public List<string> GetNotificationTypes()
    {
        if (cache.TryGetValue<List<string>>(nameof(GetNotificationTypes), out var types))
        {
            return types!;
        }

        types = GetTypes();
        cache.Set(nameof(GetNotificationTypes),
            types,
            new MemoryCacheEntryOptions
            {
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration { EvictionCallback = EvictionCallback }
                }
            }
        );

        return types;

        void EvictionCallback(
            object key,
            object? value,
            EvictionReason reason,
            object? state
        )
        {
            logger.LogInformation("Cache entry {Key} with value {Value} was evicted due to {Reason}. {State}",
                key,
                value,
                reason,
                state
            );
        }
    }

    private static List<string> GetTypes()
    {
        var assessmentTypes = Assessment.Types.Select(type => $"{type} {nameof(Assessment)}");
        return DbContextUtil.GetEntityTypes<LocalDbCtx, INotification>()
            .Where(x => x != typeof(Assessment))
            .Select(x => x.Name)
            .Concat(assessmentTypes)
            .Select(x => x.Humanize(LetterCasing.Title))
            .ToList();
    }
}