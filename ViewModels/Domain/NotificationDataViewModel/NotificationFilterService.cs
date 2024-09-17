using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Lib.Attributes;
using ReactiveUI;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;

namespace ViewModels.Domain.NotificationDataViewModel;

public interface INotificationFilterService
{
    IObservable<IPageResult> Connect(INotificationFilter fields);
    void Refresh();
}

[Inject(typeof(INotificationFilterService))]
public class NotificationFilterService(NotificationDataStreamService service) : INotificationFilterService
{
    private readonly BehaviorSubject<Unit> _refresh = new(Unit.Default);
    private readonly Subject<int> _currentPageSubject = new();

    public IObservable<IPageResult> Connect(INotificationFilter fields)
    {
        var source = CreateInputSource(fields);


        return _refresh
            .Select(_ => service.GetPageData(source))
            .Switch()
            .StartWith(new EmptyPageResult());
    }

    public void Refresh()
    {
        _refresh.OnNext(_refresh.Value);
    }

    private InputSource CreateInputSource(INotificationFilter input)
    {
        var f = new NotificationFilterInputSourceFactory(input);
        var (textFilter, typeFilter) = f.CreateTextFilters();
        var inputSource = new InputSource
        {
            DateFilter = f.CreateDateFilterSource(),
            TextFilter = textFilter,
            TypeFilter = typeFilter,
            PickerFilter = f.CreatePickerFilterSource(),
            PageSize = input.WhenAnyValue(x => x.PageSize),
            CurrentPage = Observable.Return(1)
        };

        inputSource.WhenAnyObservable(
                x => x.DateFilter,
                x => x.TextFilter,
                x => x.TypeFilter,
                x => x.PickerFilter,
                x => x.PageSize,
                // @formatter:off
                (_, _, _, _, _) => Unit.Default
                // @formatter:on
            )
            .Subscribe(_ => { _currentPageSubject.OnNext(1); });

        var currentPage = input
            .WhenAnyValue(x => x.CurrentPage)
            .Merge(_currentPageSubject);

        return inputSource with { CurrentPage = currentPage };
    }
}