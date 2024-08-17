using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
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
        Db = GetDb();
        NavMock = new Mock<INavigationService>();
        AppMock = new Mock<IAppService>();
        Model = new InstructorFormViewModel(factory: Resolve<ILocalDbCtxFactory>(), NavMock.Object, AppMock.Object);
    }

    public Mock<IAppService> AppMock { get; set; }

    public Mock<INavigationService> NavMock { get; set; }

    public InstructorFormViewModel Model { get; set; }

    public LocalDbCtx Db { get; set; }

    [TearDown]
    public async Task TearDown()
    {
        await Db.DisposeAsync();
    }


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

    private static void SetValidValues(IUserField fields)
    {
        fields.Name = "Name Abcd";
        fields.Phone = "(555) 123-4567";
        fields.Email = "Name12345@mail.com";
    }

    [Test]
    public async Task SaveAsync_DuplicateEmailAdding_AlertsApp()
    {
        var first = Db.Instructors.First();
        Model.SetAdding();
        SetValidValues(Model);
        Model.Email = first.Email;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));
    }

    [Test]
    public async Task SaveAsync_DuplicateEmailEditing_AlertsApp()
    {
        var first = Db.Instructors.First();
        Model.SetEditing(3);
        SetValidValues(Model);
        Model.Email = first.Email;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));
    }


    [TestCase("", "", "")]
    [TestCase("Test 123", "abc@mail.com", "")]
    [TestCase("Test 123", "", "2222222")]
    [TestCase("", "abc@mail.com", "2222222")]
    public async Task SaveAsync_AnyEmptyFields_AlertsApp(string name, string email, string phone)
    {
        Model.SetAdding();
        Model.Name = name;
        Model.Email = email;
        Model.Phone = phone;
        await Model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);
    }


    private static IEnumerable<TestCaseData> InstructorAdd()
    {
        var res = new List<Instructor>
        {
            new()
            {
                Name = "Anika Patel",
                Email = "anika.patel@strimeuniversity.edu",
                Phone = "555-123-4567"
            },
            new()
            {
                Name = "Test 123",
                Email = "abcdefgh@email.com",
                Phone = "(555) 222-2222"
            },
            new()
            {
                Name = "a",
                Email = "abc@mail.com",
                Phone = "(111) 222-2222"
            }
        };

        var res2 = res.Concat(new TestDataFactory().CreateInstructors()).ToList();


        foreach (var x in res2)
        {
            x.Name += "a";
            x.Email = "a" + x.Email;
        }


        return res2.Select(x => new TestCaseData(x));
    }

    private static IEnumerable<TestCaseData> InstructorEdit()
    {
        var factory = new TestDataFactory();
        return factory.CreateInstructors().Select(x => new TestCaseData(x.Id));
    }


    [TestCaseSource(nameof(InstructorAdd))]
    public async Task SaveAsync_Add_Valid_PersistsChanges(Instructor arg)
    {
        Model.SetAdding();
        Model.Name = arg.Name;
        Model.Email = arg.Email;
        Model.Phone = arg.Phone;

        using var scope = new AssertionScope();


        await Model.SaveAsync();

        var res = await Db.Instructors.Where(x => x.Name == arg.Name).ToListAsync();
        res.Should().ContainSingle();

        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Never);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }

    [TestCaseSource(nameof(InstructorEdit))]
    public async Task SaveAsync_Edit_Valid_PersistsChanges(int id)
    {
        Model.SetEditing(id);
        await Model.Init(id);


        using var scope = new AssertionScope();


        await Model.SaveAsync();

        var res = await Db.Instructors.Where(x => x.Name == Model.Name).ToListAsync();
        res.Should().ContainSingle();

        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Never);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }
}