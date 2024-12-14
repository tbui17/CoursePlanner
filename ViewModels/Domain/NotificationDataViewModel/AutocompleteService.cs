using Lib.Attributes;
using Lib.Services.NotificationService;

namespace ViewModels.Domain.NotificationDataViewModel;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class AutocompleteService(NotificationTypeProvider provider)
{
    public IList<string> GetNotificationTypes()
    {
        return provider.GetNotificationTypes();
    }
}