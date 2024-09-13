using System.Reactive.Linq;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Extensions;
using Lib.Models;
using Microsoft.Extensions.Logging.Testing;
using ReactiveUI;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Models;
using ViewModels.Services.NotificationDataStreamFactory;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

[Timeout(10000)]
public class NotificationDataStreamFactoryTest : BaseTest
{
    private InputSource CreateDefaultInputSource()
    {
        return new InputSource
        {
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
}