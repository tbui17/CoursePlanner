using Lib.Models;
using Moq;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace ViewModelTests;

public class InstructorFormViewModelTests : BaseDbTest
{

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Db = GetDbFactory()();
        NavMock = new Mock<INavigationService>();
        AppMock = new Mock<IAppService>();
        Model = new InstructorFormViewModel(factory: Resolve<ILocalDbCtxFactory>(), NavMock.Object, AppMock.Object);
    }

    public Mock<IAppService> AppMock { get; set; }

    public Mock<INavigationService> NavMock { get; set; }

    public InstructorFormViewModel Model { get; set; }

    public LocalDbCtx Db { get; set; }




    [Test]
    public async Task SaveAsync_InvalidPhone_AlertsApp()
    {
        Model.SetAdding();
        Model.Name = "Test 123";
        Model.Email = "asdjqisfj@mail.com";
        Model.Phone = "aaaaaa";
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SaveAsync_DuplicateEmailAdding_AlertsApp()
    {
        var first = Db.Instructors.First();
        Model.SetAdding();
        ((IInstructorFormFields)Model).SetValidValues();
        Model.Email = first.Email;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));

    }

    [Test]
    public async Task SaveAsync_DuplicateEmailEditing_AlertsApp()
    {
        var first = Db.Instructors.First();
        Model.SetEditing(3);
        ((IInstructorFormFields)Model).SetValidValues();
        Model.Email = first.Email;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));
    }


    [TestCase("","","")]
    [TestCase("Test 123","abc@mail.com","")]
    [TestCase("Test 123","","2222222")]
    [TestCase("","abc@mail.com","2222222")]
    public async Task SaveAsync_AnyEmptyFields_AlertsApp(string name, string email, string phone)
    {
        Model.SetAdding();
        Model.Name = name;
        Model.Email = email;
        Model.Phone = phone;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);

    }


}