using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using AutoFixture;
using BaseTestSetup.Lib;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using Lib.Services.NotificationService;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Reactive.Testing;
using ReactiveUI;
using ViewModels.Domain;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

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
    public async Task Initialization_ProducesPaginatedData()
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
            .ToTask(new CancellationTokenSource(5000).Token);

        using var _ = new AssertionScope();

        var pageCount = (data.Count / 10).Should().Be(5).And.Subject;

        res.PageCount.Should().Be(pageCount);

        res.CurrentPage.Should().Be(1);

        res.CurrentPageData.Should().HaveCount(10);
        res.ItemCount.Should().Be(10);
    }
}