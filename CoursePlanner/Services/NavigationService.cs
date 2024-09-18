using CoursePlanner.Pages;
using Lib.Interfaces;
using Lib.Models;
using Microsoft.Extensions.Logging;
using ViewModels.Domain;
using ViewModels.Interfaces;
using ViewModels.Services;

namespace CoursePlanner.Services;

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<AppService> _logger;

    public NavigationService(IServiceProvider provider, ILogger<AppService> logger)
    {
        _provider = provider;
        _logger = logger;
        Current.Navigating += OnNavigating;
        Current.Navigated += OnNavigated;
    }

    private async void OnNavigated(object? sender, ShellNavigatedEventArgs args)
    {
        _logger.LogInformation(
            "{Method} {Sender} {Source} {PreviousLocation} {CurrentLocation} {CurrentTitle} {CurrentItemCount}",
            nameof(OnNavigated),
            sender,
            args.Source,
            args.Previous?.Location,
            args.Current?.Location,
            Shell.Current?.CurrentPage?.Title,
            Shell.Current?.Items?.Count
        );

        if (GetRefreshable() is not { } refreshable)
        {
            return;
        }

        _logger.LogInformation("Refreshing page for shell navigation: {PageName}", Current.CurrentPage.Title);
        await refreshable.Model.RefreshAsync();

        return;

        IRefreshableView<IRefresh>? GetRefreshable()
        {

            if (Current.CurrentPage is not IRefreshableView<IRefresh> refreshable2)
            {
                return null;
            }

            return args.Source
                is ShellNavigationSource.Pop
                or ShellNavigationSource.PopToRoot
                or ShellNavigationSource.ShellContentChanged
                or ShellNavigationSource.ShellItemChanged
                or ShellNavigationSource.ShellSectionChanged
                ? refreshable2
                : null;
        }
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs args)
    {
        _logger.LogDebug(
            "{Method} {Sender} {Source} {CurrentLocation} {CurrentTitle} {CurrentItemCount}",
            nameof(OnNavigated),
            sender,
            args.Source,
            args.Current?.Location,
            Shell.Current?.CurrentPage?.Title,
            Shell.Current?.Items?.Count
        );
        if (args.Source is not ShellNavigationSource.Pop
            || Navigation.NavigationStack is not [.., IRefreshableView<IRefresh> refreshable, not null]
           )
        {
            return;
        }


        _logger.LogInformation("Refreshing page for back navigation: {PageName}", Current.CurrentPage.Title);
        await refreshable.Model.RefreshAsync();
    }

    private T Resolve<T>() where T : notnull => _provider.GetRequiredService<T>();

    private static Shell Current => Shell.Current;

    private static INavigation Navigation => Current.Navigation;

    private async Task PushAsync(Page page)
    {
        _logger.LogInformation("Navigating to page {PageName}", page.Title);
        await Navigation.PushAsync(page);
    }

    private async Task GoToAsync<T>(int id) where T : Page, IRefreshableView<IRefreshId>
    {
        var page = Resolve<T>();
        await GoToAsync(page, id);
    }

    private async Task GoToAsync<T>(T page, int id) where T : Page, IRefreshableView<IRefreshId>
    {
        _logger.LogInformation("Initializing model {ModelName} with id {Id} for {PageTitle}", page.Model.GetType().Name, id, page.Title);
        await page.Model.Init(id);
        await PushAsync(page);
    }

    public async Task GoToNotificationDetailsPage(INotification notification)
    {
        var t = notification switch
        {
            Course x => GoToDetailedCoursesPageAsync(x.Id),
            Assessment x => GoToAssessmentDetailsPageAsync(x.CourseId),
            _ => throw new ArgumentOutOfRangeException(nameof(notification), notification, "Unknown item type")
        };
        await t;
    }

    private async Task GoToAsync<T>(T page) where T : Page
    {
        if (page is IRefreshableView<IRefresh> refreshable)
        {
            await refreshable.Model.RefreshAsync();
        }

        await PushAsync(page);
    }


    public async Task GoToMainPageAsync()
    {
        if (Current.CurrentPage is MainPage)
        {
            _logger.LogInformation("Already on main page.");
            var dummy = new ContentPage();
            await GoToAsync(dummy);
        }

        const string pageName = nameof(MainPage);
        _logger.LogInformation("Navigating to main page");
        await Current.GoToAsync($"///{pageName}");
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