using System.Diagnostics.CodeAnalysis;
using Humanizer;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Lib.Services.NotificationService;

[SuppressMessage("ReSharper", "RedundantNameQualifier")]
[Inject(Lifetime = ServiceLifetime.Singleton)]
public class NotificationTypeProvider(IMemoryCache cache)
{
    public List<string> GetNotificationTypes()
    {

        if (!cache.TryGetValue<List<string>>(nameof(GetNotificationTypes), out var types))
        {

            types = GetTypes();
            cache.Set(nameof(GetNotificationTypes), types);
            return types;

        }


        return types!;

        List<string> GetTypes()
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


}