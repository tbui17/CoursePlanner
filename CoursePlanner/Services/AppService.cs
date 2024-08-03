using Lib.Exceptions;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CoursePlanner.Services;

public class AppService
{
    private readonly IServiceProvider _provider;
    private readonly ILocalNotificationService _localNotificationService;


    // ReSharper disable once NotAccessedField.Local
    private readonly ILogger<AppService> _logger;

    public AppService(IServiceProvider provider, ILocalNotificationService localNotificationService, ILogger<AppService> logger)
    {
        _provider = provider;
        _localNotificationService = localNotificationService;
        _logger = logger;
        StartScheduledTasks();
    }

    // ReSharper disable once NotAccessedField.Local Avoid losing reference to timer
    private Timer? _notificationJob;

    private void StartScheduledTasks()
    {

        var time = TimeSpan.FromDays(1);
        _logger.LogInformation("Created notification timer with {Time}",time);
        _notificationJob = new Timer(
            NotifyTask,
            null,
            TimeSpan.Zero,
            time
        );
        return;
        async void NotifyTask(object? _) => await _localNotificationService.Notify();
    }



    public async Task ShareAsync(ShareTextRequest request)
    {
        await Share.RequestAsync(request);
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

    public async Task GoToEditCoursePageAsync(int courseId)
    {
        await GoToAsync<EditCoursePage>(x => x.Model.Id = courseId);
    }

    public async Task GoToAddInstructorPageAsync()
    {
        var page = Resolve<InstructorFormPage>();
        page.Model.SetAdding();

        await Navigation.PushAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int instructorId)
    {
        var page = Resolve<InstructorFormPage>();

        page.Model.SetEditing(instructorId);

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

    public async Task AlertAsync(string message)
    {
        await Current.DisplayAlert("Alert", message, "OK");
    }
}