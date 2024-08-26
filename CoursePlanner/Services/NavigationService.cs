using CoursePlanner.Pages;
using Microsoft.Extensions.Logging;
using ViewModels.Events;
using ViewModels.Interfaces;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace CoursePlanner.Services;



public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<AppService> _logger;
    private readonly NavigationSubject _subject;

    public NavigationService(IServiceProvider provider,
        ILogger<AppService> logger,
        NavigationSubject subject)
    {
        _provider = provider;
        _logger = logger;
        _subject = subject;
        Current.Navigated += OnNavigated;



    }

    private void OnNavigated(object? _, object? __)
    {
        var currentPage = Current.CurrentPage;
        if (currentPage is null)
        {
            _logger.LogWarning("Navigated to null page");
            return;
        }
        var pageType = currentPage.GetType();
        _logger.LogInformation("Navigated to {PageName}", pageType.Name);
        if (currentPage is not IRefreshableView<IRefresh> refreshable) return;
        _logger.LogInformation("Refreshing {PageName}", pageType.Name);
        refreshable.Model.RefreshAsync();

    }

    private T Resolve<T>() where T : notnull => _provider.GetRequiredService<T>();

    private static Shell Current => Shell.Current;

    private static INavigation Navigation => Current.Navigation;

    private async Task PushAsync(Page page)
    {
        _logger.LogInformation("Navigating to page {PageName}", page.GetType().Name);
        await Navigation.PushAsync(page);
    }

    private async Task GoToAsync<T>(int id) where T : Page, IRefreshableView<IRefresh>
    {
        _logger.LogInformation("Navigating to {PageName}", typeof(T).Name);
        var page = Resolve<T>();

        await GoToAsync(page, id);
    }

    private async Task GoToAsync<T>(T page, int id) where T : Page, IRefreshableView<IRefresh>
    {

        _subject.Publish(new NavigationEventArg(typeof(T), id));
        await PushAsync(page);
    }

    private async Task GoToAsync<T>(T page) where T : Page
    {
        await PushAsync(page);
    }


    public async Task GoToMainPageAsync()
    {
        if (Current.CurrentPage is MainPage)
        {
            _logger.LogInformation("Already on main page. Creating dummy page to force navigation.");
            var dummy = new ContentPage();
            await GoToAsync(dummy);
        }

        const string pageName = nameof(MainPage);
        _logger.LogInformation("Navigating to main page");
        await Current.GoToAsync($"///{pageName}");
        _subject.Publish(new NavigationEventArg(typeof(MainPage)));
    }



    public async Task GoToDetailedTermPageAsync(int id)
    {
        await GoToAsync<DetailedTermPage>(id);
    }

    public async Task GoToDetailedCoursesPageAsync(int id)
    {
        await GoToAsync<DetailedCoursePage>(id);
    }

    public async Task GoToEditTermPageAsync(int id)
    {
        await GoToAsync<EditTermPage>(id);
    }

    public async Task GoToEditCoursePageAsync(int id)
    {
        await GoToAsync<EditCoursePage>(id);
    }

    public async Task GoToAddInstructorPageAsync()
    {
        _logger.LogInformation("Navigating to add instructor page");
        var factory = Resolve<InstructorFormViewModelFactory>();
        var page = new InstructorFormPage(factory.CreateAddingModel());
        await GoToAsync(page);
    }

    public async Task GotoEditInstructorPageAsync(int id)
    {
        _logger.LogInformation("Navigating to edit instructor page with id {Id}", id);
        var factory = Resolve<InstructorFormViewModelFactory>();
        var page = new InstructorFormPage(await factory.CreateInitializedEditingModel(id));
        await GoToAsync(page, id);
    }

    public async Task GoToAssessmentDetailsPageAsync(int id)
    {
        await GoToAsync<EditAssessmentPage>(id);
    }

    public async Task GoToNoteDetailsPageAsync(int id)
    {
        await GoToAsync<EditNotePage>(id);
    }

    public async Task PopAsync()
    {
        await Navigation.PopAsync();
    }
}