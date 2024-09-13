using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Attributes;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationFilterService
{
    IObservable<IPageResult> Connect(INotificationFilter fields);
    void Refresh();
}

[Inject(typeof(INotificationFilterService))]
public class NotificationFilterService(NotificationDataStreamFactory factory) : INotificationFilterService
{
    private readonly BehaviorSubject<Unit> _refresh = new(Unit.Default);

    public IObservable<IPageResult> Connect(INotificationFilter fields)
    {
        var source = new NotificationFilterInputSourceFactory(fields).CreateInputSource();

        return _refresh
            .Select(_ => factory.CreatePageDataStream(source))
            .Switch();
    }

    public void Refresh()
    {
        _refresh.OnNext(_refresh.Value);
    }
}