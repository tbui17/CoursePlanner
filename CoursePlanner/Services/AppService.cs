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


    private async Task Notify()
    {
        var notifications = (await _notificationService.GetNotifications()).ToList();

        if (notifications.Count == 0)
        {
            return;
        }

        var notificationRequest = CreateNotificationRequest(notifications);

        await LocalNotificationCenter.Current.Show(notificationRequest);

        return;

        static NotificationRequest CreateNotificationRequest(IReadOnlyList<NotificationResult> notifications) =>
            new()
            {
                NotificationId = 1,
                Title = "Upcoming Events",
                Description = $"You have {notifications.Count} upcoming events.",
                BadgeNumber = notifications.Count,
                Subtitle = notifications.ToMessage(),
            };
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

        page.Model.InstructorPersistence = async instructor =>
        {
            if (instructor.Validate() is { } e)
            {
                return e;
            }

            await using var db = await _factory.CreateDbContextAsync();

            db.Instructors.Add(instructor);
            await db.SaveChangesAsync();

            return null;
        };

        await Navigation.PushAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int instructorId)
    {
        var page = Resolve<InstructorFormPage>();

        page.Model.InstructorPersistence = async instructor =>
        {
            if (instructor.Validate() is { } e)
            {
                return e;
            }

            await using var db = await _factory.CreateDbContextAsync();

            var editModel = await db.Instructors.FirstAsync(x => x.Id == instructorId);

            editModel.Name = instructor.Name;
            editModel.Email = instructor.Email;
            editModel.Phone = instructor.Phone;
            await db.SaveChangesAsync();
            return null;
        };

        await Navigation.PushAsync(page);
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
}