using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Moq;
using ViewModels.Domain;
using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.ViewModelTestGroup;

public class InstructorFormViewModelTests : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();

        ViewModelFactory =
            new InstructorFormViewModelFactory(factory: Resolve<ILocalDbCtxFactory>(), NavMock.Object, AppMock.Object);
    }

    private InstructorFormViewModelFactory ViewModelFactory { get; set; }

    [Test]
    public async Task SaveAsync_InvalidPhone_AlertsApp()
    {
        var model = ViewModelFactory.CreateAddingModel();
        model.Name = "Test 123";
        model.Email = "asdjqisfj@mail.com";
        model.Phone = "aaaaaa";
        await model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Once);
    }

    private static void SetValidValues(IContactForm fields)
    {
        fields.Name = "Name Abcd";
        fields.Phone = "(555) 123-4567";
        fields.Email = "Name12345@mail.com";
    }

    [Test]
    public async Task SaveAsync_DuplicateEmailAdding_AlertsApp()
    {
        var model = ViewModelFactory.CreateAddingModel();
        var first = Db.Instructors.First();
        SetValidValues(model);
        model.Email = first.Email;
        await model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));
    }

    [Test]
    public async Task SaveAsync_DuplicateEmailEditing_AlertsApp()
    {
        var model = await ViewModelFactory.CreateInitializedEditingModel(3);
        var first = Db.Instructors.First(x => x.Id == 1);
        SetValidValues(model);
        model.Email = first.Email;
        await model.SaveAsync();
        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()));
    }


    [TestCase("", "", "")]
    [TestCase("Test 123", "abc@mail.com", "")]
    [TestCase("Test 123", "", "2222222")]
    [TestCase("", "abc@mail.com", "2222222")]
    public async Task SaveAsync_AnyEmptyFields_AlertsApp(string name, string email, string phone)
    {
        var model = ViewModelFactory.CreateAddingModel();
        model.Name = name;
        model.Email = email;
        model.Phone = phone;
        await model.SaveAsync();
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
        var model = ViewModelFactory.CreateAddingModel();
        model.Name = arg.Name;
        model.Email = arg.Email;
        model.Phone = arg.Phone;

        using var scope = new AssertionScope();


        await model.SaveAsync();

        var res = await Db.Instructors.Where(x => x.Name == arg.Name).ToListAsync();
        res.Should().ContainSingle();

        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Never);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }

    [TestCaseSource(nameof(InstructorEdit))]
    public async Task SaveAsync_Edit_Valid_PersistsChanges(int id)
    {
        var initial = await Db.Instructors.AsNoTracking().FirstAsync(x => x.Id == id);
        var unexpected = new { initial.Name, initial.Email, initial.Phone };
        var model = await ViewModelFactory.CreateInitializedEditingModel(id);

        const string name = "Amnfoiwjefi2";

        model.Name = name;
        model.Email = "abcde@mail.com";
        model.Phone = "(888) 123-4567";


        using var scope = new AssertionScope();


        await model.SaveAsync();

        var res = await Db.Instructors.Where(x => x.Name == name).ToListAsync();
        var subj = res.Should().ContainSingle().Subject;
        subj.Should().NotBeEquivalentTo(unexpected);


        AppMock.Verify(x => x.ShowErrorAsync(It.IsAny<string>()), Times.Never);
        NavMock.Verify(x => x.PopAsync(), Times.Once);
    }
}