using CoursePlanner.Pages;
using Microsoft.Extensions.Logging;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace CoursePlanner.Services;

public class NavigationService(
    IServiceProvider provider,
    ILogger<AppService> logger) : INavigationService
{
    private T Resolve<T>() where T : notnull => provider.GetRequiredService<T>();

    private static Shell Current => Shell.Current;

    private static INavigation Navigation => Current.Navigation;


    private async Task GoToAsync<T>(Func<T, Task> setter) where T : Page
    {
        logger.LogInformation("Navigating to {PageName}", typeof(T).Name);
        var page = Resolve<T>();
        await setter(page);
        await Navigation.PushAsync(page);
    }

    public async Task GoToMainPageAsync()
    {
        if (Current.CurrentPage is MainPage)
        {
            logger.LogInformation("Already on main page. Creating dummy page to force navigation.");
            var dummy = new ContentPage();
            await Navigation.PushAsync(dummy);
        }
        logger.LogInformation("Navigating to main page");

        await GoToAsync(nameof(MainPage));
    }

    private async Task GoToAsync(string pageName)
    {
        logger.LogInformation("Navigating by shell to {PageName}", pageName);
        await Current.GoToAsync($"///{pageName}");
    }

    public async Task GoToDetailedTermPageAsync(int id)
    {
        await GoToAsync<DetailedTermPage>(async x => await x.Model.Init(id));
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
        logger.LogInformation("Navigating to add instructor page");
        var factory = Resolve<InstructorFormViewModelFactory>();
        var page = new InstructorFormPage(factory.CreateAddingModel());
        await Navigation.PushAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int id)
    {
        logger.LogInformation("Navigating to edit instructor page with id {Id}", id);
        var factory = Resolve<InstructorFormViewModelFactory>();
        var page = new InstructorFormPage(await factory.CreateInitializedEditingModel(id));
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
}