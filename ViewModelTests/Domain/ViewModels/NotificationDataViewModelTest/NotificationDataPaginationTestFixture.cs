using AutoFixture;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using Moq;
using ViewModels.Domain;
using ViewModels.Domain.NotificationDataViewModel;
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
        await Model.Should().EventuallyHave(x => x.PageResult is { CurrentPageData.Count: > 0 });
    }


    public IReadOnlyCollection<INotification> GetSubset(int index)
    {
        return Data
            .Chunk(10)
            .ElementAt(index)
            .ToList();
    }

    public List<int> GetIdSubset(int index)
    {
        return GetSubset(index)
            .Select(x => x.Id)
            .ToList();
    }
}