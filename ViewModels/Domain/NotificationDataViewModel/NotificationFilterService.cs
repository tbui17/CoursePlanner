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
    IObservable<int> CurrentPageOverridden { get; }
    IObservable<IPageResult> Connect(INotificationFilter fields);
    void Refresh();
}

[Inject(typeof(INotificationFilterService))]
public class NotificationFilterService(NotificationDataStreamService service) : INotificationFilterService
{
    private readonly BehaviorSubject<Unit> _refresh = new(Unit.Default);
    private readonly Subject<int> _currentPageOverridden = new();
    public IObservable<int> CurrentPageOverridden => _currentPageOverridden;

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
        var factory = new NotificationFilterInputSourceFactory(input);
        var (textFilter, typeFilter) = factory.CreateTextFilters();
        var inputSource = new InputSource
        {
            DateFilter = factory.CreateDateFilterSource(),
            TextFilter = textFilter,
            TypeFilter = typeFilter,
            PickerFilter = factory.CreatePickerFilterSource(),
            PageSize = input.WhenAnyValue(x => x.PageSize),
            CurrentPage = Observable.Empty<int>()
        };

        OnAnyFieldExceptCurrentPage();


        var currentPage = input
            .WhenAnyValue(x => x.CurrentPage) // by default WhenAnyValue applies DistinctUntilChanged
            .Merge(CurrentPageOverridden);

        return inputSource with { CurrentPage = currentPage };

        void OnAnyFieldExceptCurrentPage()
        {
            // force current page to 1 when any filter changes
            // vm needs to be aware and set current page to 1 on exposed observable
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
                .Subscribe(_ => { _currentPageOverridden.OnNext(1); });
        }
    }
}