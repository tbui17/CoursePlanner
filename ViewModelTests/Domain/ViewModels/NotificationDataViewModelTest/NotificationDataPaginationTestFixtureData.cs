using AutoFixture;
using Lib.Interfaces;
using Lib.Services.NotificationService;
using Moq;
using ViewModels.Domain;

namespace ViewModelTests.Domain.ViewModels.NotificationDataViewModelTest;

public record NotificationDataPaginationTestFixtureData
{
    public required IFixture Fixture { get; set; }
    public required List<INotification> Data { get; set; }
    public required Mock<INotificationDataService> DataService { get; set; }
    public required NotificationDataViewModel Model { get; set; }
    public List<int> DataIds => Data.Select(x => x.Id).ToList();
    public List<int> ExpectedIds => Data.Select(x => x.Id).ToList();
    public required List<INotification> Expected { get; set; }
}