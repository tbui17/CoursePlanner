using CoursePlanner.Pages;
using CoursePlanner.Utils;
using Microsoft.Extensions.Logging;
using ViewModels.Services;

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

namespace CoursePlanner.Services;

public class AppService : IAppService, INavigationService
{
    private readonly IServiceProvider _provider;


    // ReSharper disable once NotAccessedField.Local
    private readonly ILogger<AppService> _logger;
    private readonly PageResolver _pageResolver;

    public AppService(
        IServiceProvider provider,
        ILocalNotificationService localNotificationService,
        ILogger<AppService> logger,
        PageResolver pageResolver
    )
    {
        _provider = provider;
        _logger = logger;
        localNotificationService.StartListening();
        _pageResolver = pageResolver;
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


    private async Task GoToAsync<T>(Func<T,Task> setter) where T : Page
    {
        var page = Resolve<T>();
        await setter(page);
        await Navigation.PushAsync(page);
    }


    public async Task GoToAsync(NavigationTarget target)
    {
        if (target == NavigationTarget.MainPage)
        {
            // temp bandaid until class is refactored
            // removes extra stack in nav history
            // trigger main page rerender
            await Navigation.PushAsync(new ContentPage());
            await Navigation.PopAsync();
            return;
        }
        var page = _pageResolver.GetPage(target);
        await Navigation.PushAsync(page);
    }

    public async Task GoToDetailedTermPageAsync(int id)
    {
        await GoToAsync<DetailedTermPage>(async x => await x.Model.Init(id));
    }

    public async Task<string?> DisplayNamePromptAsync()
    {
        return await Current.CurrentPage.DisplayPromptAsync("Enter name", "") switch
        {
            { } name when !string.IsNullOrWhiteSpace(name) => name,
            _ => null
        };
    }

    public async Task GoToDetailedCoursesPageAsync(int id)
    {
        await GoToAsync<DetailedCoursePage>(x => x.Model.Init(id));
    }

    public async Task GoToEditTermPageAsync(int id)
    {
        await GoToAsync<EditTermPage>(x => x.Model.Init(id));
    }

    public async Task GoToEditCoursePageAsync(int id)
    {
        await GoToAsync<EditCoursePage>(x => x.Model.Init(id));
    }

    public async Task GoToAddInstructorPageAsync()
    {
        var page = Resolve<InstructorFormPage>();
        page.Model.SetAdding();

        await Navigation.PushAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int id)
    {
        var page = Resolve<InstructorFormPage>();

        page.Model.SetEditing(id);

        await Navigation.PushAsync(page);
    }

    public async Task GoToAssessmentDetailsPageAsync(int id)
    {
        await GoToAsync<EditAssessmentPage>(x => x.Model.Init(id));
    }

    public async Task GoToNoteDetailsPageAsync(int id)
    {
        await GoToAsync<EditNotePage>(x => x.Model.Init(id));
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