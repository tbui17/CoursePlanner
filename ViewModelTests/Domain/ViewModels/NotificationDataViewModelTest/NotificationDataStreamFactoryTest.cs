using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using Lib.Models;
using Microsoft.Extensions.Logging.Testing;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(10000)]
public class NotificationDataStreamFactoryTest : BaseTest
{
    private InputSource CreateDefaultInputSource()
    {
        return new InputSource
        {
            Refresh = new BehaviorSubject<object?>(new object()),
            CurrentPage = CreateProperty(1),
            DateFilter = CreateProperty(new DateTimeRange())!,
            PageSize = CreateProperty(10),
            TextFilter = CreateProperty("")!,
            PickerFilter = CreateProperty(ShouldNotifyIndex.None),
            TypeFilter = CreateProperty("")!
        };
    }

    private static ReactiveProperty<T> CreateProperty<T>(T value) where T : notnull
    {
        return new ReactiveProperty<T>(value);
    }


    [Test]
    public async Task CreatePageDataStream_Initialization_ProducesPaginatedData()
    {
        var f = CreateFixture();

        var (data, service) = Resolve<NotificationDataPaginationTestFixtureDataFactory>().CreateDataServiceWithData();


        var dataFactory = new NotificationDataStreamFactory(
            service.Object,
            new FakeLogger<NotificationDataStreamFactory>(),
            f.Create<PageResultFactory>()
        );

        var input = CreateDefaultInputSource();

        var obs = dataFactory.CreatePageDataStream(input);

        var res = await obs
            .FirstAsync(x => x.CurrentPageData.Count > 0)
            .Timeout(5.Seconds());

        using var _ = new AssertionScope();

        var pageCount = (data.Count / 10).Should().Be(5).And.Subject;

        res.PageCount.Should().Be(pageCount);

        res.CurrentPage.Should().Be(1);

        res.CurrentPageData.Should().HaveCount(10);
        res.ItemCount.Should().Be(10);
    }

    [Test]
    public async Task CreatePageDataStream_UniqueNameFilter_ReturnsSingleItemCollection()
    {
        var f = CreateFixture();

        var (data, service) = Resolve<NotificationDataPaginationTestFixtureDataFactory>().CreateDataServiceWithData();


        var dataFactory = new NotificationDataStreamFactory(
            service.Object,
            new FakeLogger<NotificationDataStreamFactory>(),
            f.Create<PageResultFactory>()
        );

        var input = CreateDefaultInputSource();
        var name = data[0].Name;
        var textFilter = (ReactiveProperty<string>)input.TextFilter;

        var obs = dataFactory.CreatePageDataStream(input);


        var res = await obs.FirstAsync(x => x.CurrentPageData.Count > 0)
            .Do(_ => textFilter.Value = name)
            .Concat(obs.FirstAsync(x => x.CurrentPageData.Count == 1))
            .Timeout(5.Seconds());

        using var _ = new AssertionScope();
        res.CurrentPageData.Should().ContainSingle().And.ContainSingle(x => x.Name == name);
        res.ItemCount.Should().Be(1);
        res.CurrentPage.Should().Be(1);
        res.PageCount.Should().Be(5);
    }

    [Test]
    public async Task CreatePageDataStream_Refresh_InvokesNewCallEveryTime()
    {
        var f = CreateFixture();

        var (_, service) = Resolve<NotificationDataPaginationTestFixtureDataFactory>().CreateDataServiceWithData();


        var dataFactory = new NotificationDataStreamFactory(
            service.Object,
            new FakeLogger<NotificationDataStreamFactory>(),
            f.Create<PageResultFactory>()
        );

        service.Invocations.Should().BeEmpty();

        // var refresh = new BehaviorSubject<object?>(new object());
        var input = CreateDefaultInputSource();
        var refresh = input.Refresh.As<BehaviorSubject<object?>>();


        var obs = dataFactory.CreatePageDataStream(input);
        obs.TakeWhile(_ => service.Invocations.Count < 3)
            .Subscribe(_ => refresh.OnNext(new object()));

        await service.Invocations.WaitFor(x => x.Count == 3);
        service.Invocations.Should().HaveCount(3);
    }
}