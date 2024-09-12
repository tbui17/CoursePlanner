using AutoFixture;
using FluentAssertions;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using Moq;
using ViewModels.Domain;
using ViewModelTests.Utils;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public class NotificationDataPaginationTestFixture
{
    public required IFixture Fixture { get; set; }
    public required List<INotification> Data { get; set; }
    public required Mock<INotificationDataService> DataService { get; set; }
    public required NotificationDataViewModel Model { get; set; }
    public List<int> DataIds => Data.Select(x => x.Id).ToList();
    public List<int> ExpectedIds => Data.Select(x => x.Id).ToList();
    public required List<INotification> Expected { get; set; }



    public async Task ModelEventuallyHasData()
    {
        await Model.Should()
            .EventuallySatisfy(x => { x.PageResult?.CurrentPageData.Should().HaveCountGreaterThan(10); });
    }


    public IReadOnlyCollection<INotification> GetSubset(int index)
    {
        return Data
            .Chunk(10)
            .ElementAt(index)
            .ToList();
    }
}