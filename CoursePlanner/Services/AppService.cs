using Lib.Exceptions;
using Lib.Models;
using Lib.Services;
using Microsoft.EntityFrameworkCore;
using Plugin.LocalNotification;

namespace CoursePlanner.Services;

public class AppService
{
    private readonly IServiceProvider _provider;
    private readonly ILocalDbCtxFactory _factory;
    private readonly NotificationService _notificationService;

    // ReSharper disable once NotAccessedField.Local
    private readonly Timer _notificationJob;

    public AppService(IServiceProvider provider, ILocalDbCtxFactory factory, NotificationService notificationService)
    {
        _provider = provider;
        _factory = factory;
        _notificationService = notificationService;
        _notificationJob = CreateTimer();
    }

    private Timer CreateTimer() =>
        new(
            NotifyTask,
            null,
            TimeSpan.Zero,
            TimeSpan.FromDays(1)
        );

    private async void NotifyTask(object? _) => await Notify();

    public async Task ShareAsync(ShareTextRequest request)
    {
        await Share.RequestAsync(request);
    }


    public async Task Notify()
    {
        var notifications = (await _notificationService.GetNotifications()).ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        var notificationRequest = CreateNotificationRequest(notifications);

        await LocalNotificationCenter.Current.Show(notificationRequest);

        return;

        static NotificationRequest CreateNotificationRequest(IReadOnlyList<NotificationResult> notifications)
        {
            var title = $"{notifications.Count} upcoming events.";
            var description = title + "\n" + notifications.ToMessage();


            return new()
            {
                NotificationId = 1,
                Title = title,
                Description = description,
                BadgeNumber = notifications.Count,
            };
        }
    }


    private T Resolve<T>() where T : notnull => _provider.GetRequiredService<T>();

    private static Shell Current => Shell.Current;

    private static INavigation Navigation => Current.Navigation;

    public async Task ShowErrorAsync(string message)
    {
        await Current.DisplayAlert("Error", message, "OK");
    }


    private async Task GoToAsync<T>(Func<T, int> setter) where T : Page
    {
        var page = Resolve<T>();
        setter(page);
        await Navigation.PushAsync(page);
    }


    public async Task GoToMainPageAsync()
    {
        await Current.GoToAsync($"///{nameof(MainPage)}");
    }

    private static async Task GoToAsync<T>(IDictionary<string, object>? query = null) where T : ContentPage
    {
        var route = $"{typeof(T).Name}";
        if (query is not null)
        {
            await Current.GoToAsync(route, query);
        }
        else
        {
            await Current.GoToAsync(route);
        }
    }

    public async Task GoToDetailedTermPageAsync(int termId)
    {
        await GoToAsync<DetailedTermPage>(x => x.Model.Id = termId);
    }

    public async Task GoBackToDetailedTermPageAsync()
    {
        await Navigation.PopAsync();
    }

    public async Task<string?> DisplayNamePromptAsync()
    {
        return await Current.CurrentPage.DisplayPromptAsync("Enter name", "");
    }

    public async Task GoToDetailedCoursesPageAsync(int id)
    {
        await GoToAsync<DetailedCoursePage>(x => x.Model.Id = id);
    }

    public async Task GoToEditTermPageAsync(int id)
    {
        await GoToAsync<EditTermPage>(x => x.Model.Id = id);
    }

    public async Task GoBackToDetailedCoursePageAsync()
    {
        await Navigation.PopAsync();
    }

    public async Task GoToEditCoursePageAsync(int courseId)
    {
        await GoToAsync<EditCoursePage>(x => x.Model.Id = courseId);
    }

    public async Task GoToAddInstructorPageAsync()
    {
        var page = Resolve<InstructorFormPage>();


        page.Model.Title = "Add Instructor";

        page.Model.InstructorPersistence = async instructor =>
        {
            if (instructor.Validate() is { } e)
            {
                return e;
            }

            await using var db = await _factory.CreateDbContextAsync();

            if (await ValidateNoDuplicateEmail(db, instructor.Email) is { } exc)
            {
                return exc;
            }

            db.Instructors.Add(instructor);
            await db.SaveChangesAsync();

            return null;
        };

        await Navigation.PushAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int instructorId)
    {
        var page = Resolve<InstructorFormPage>();

        page.Model.Title = "Edit Instructor";
        page.Model.Id = instructorId;

        page.Model.InstructorPersistence = async instructor =>
        {
            if (instructor.Validate() is { } e)
            {
                return e;
            }


            await using var db = await _factory.CreateDbContextAsync();

            if (await ValidateNoDuplicateEmail(db, instructor.Email, instructorId) is { } exc)
            {
                return exc;
            }

            var editModel = await db.Instructors.FirstAsync(x => x.Id == instructorId);

            editModel.Name = instructor.Name;
            editModel.Email = instructor.Email;
            editModel.Phone = instructor.Phone;
            await db.SaveChangesAsync();
            return null;
        };

        await Navigation.PushAsync(page);
    }

    private static async Task<DomainException?> ValidateNoDuplicateEmail(LocalDbCtx db, string email, int? id = null)
    {
        email = email.ToLower();
        var baseQuery = db.Instructors.Where(x => x.Email.ToLower() == email);

        if (id is { } instructorId)
        {
            baseQuery = baseQuery.Where(x => x.Id != instructorId);
        }

        var maybeDuplicateEmail = await baseQuery
           .Select(x => x.Email)
           .FirstOrDefaultAsync();

        return maybeDuplicateEmail is not null
            ? new DomainException("Email already exists.")
            : null;
    }

    public async Task GoToAssessmentDetailsPageAsync(int assessmentId)
    {
        await GoToAsync<EditAssessmentPage>(x => x.Model.Id = assessmentId);
    }

    public async Task GoToNoteDetailsPageAsync(int noteId)
    {
        await GoToAsync<EditNotePage>(x => x.Model.Id = noteId);
    }

    public async Task PopAsync()
    {
        await Navigation.PopAsync();
    }

    public async Task AlertAsync(string message)
    {
        await Current.DisplayAlert("Alert", message, "OK");
    }
}