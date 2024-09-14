using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using Lib.Attributes;
using Lib.Services.NotificationService;
using ReactiveUI;
using ViewModels.Config;

namespace ViewModels.Domain.NotificationDataViewModel;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class AutocompleteService
{
    private readonly SourceCache<string, string> _source = new(x => x);

    public AutocompleteService(NotificationTypeProvider provider)
    {
        _source.AddOrUpdate(provider.GetNotificationTypes());
    }

    public IObservable<IChangeSet<string, string>> Connect()
    {
        return _source.Connect();
    }

    public ReadOnlyObservableCollection<string> BindSubscribe()
    {
        Connect()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out var collection)
            .Subscribe();
        return collection;
    }
}