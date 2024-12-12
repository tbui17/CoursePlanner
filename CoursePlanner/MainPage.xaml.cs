using CoursePlanner.Views;
using Microsoft.Extensions.Logging;
using ViewModels.Domain;
using ViewModels.Services;

namespace CoursePlanner;

public partial class MainPage
{
    private readonly ILocalNotificationService _localNotificationService;
    private readonly ILogger<MainPage> _logger;
    private readonly IServiceProvider _provider;

    private readonly ISessionService _sessionService;

    public MainPage(
        MainViewModel model,
        IServiceProvider provider,
        ILocalNotificationService localNotificationService,
        ISessionService sessionService,
        ILogger<MainPage> logger
    )
    {
        _sessionService = sessionService;
        _logger = logger;
        _provider = provider;
        _localNotificationService = localNotificationService;
        Model = model;
        InitializeComponent();
        BindingContext = this;
        _logger.LogInformation("Initialized main page");
    }

    public MainViewModel Model { get; set; }

    private void SetView(IView view)
    {
        _logger.LogInformation("Setting view {Type}", view);
        MainLayout.Clear();
        MainLayout.Add(view);
    }

    private async Task SetViewForPage()
    {
        _logger.LogInformation("User sign in status: {IsLoggedIn} {Username}",
            _sessionService.IsLoggedIn,
            _sessionService.User?.Username
        );
        if (_sessionService.IsLoggedIn)
        {
            var view = _provider.GetRequiredService<TermListView>();
            await view.Model.RefreshAsync();
            SetView(view);
            return;
        }

        SetView(_provider.GetRequiredService<LoginView>());
    }

    protected override async void OnAppearing()
    {
        // sets the view for the page based on the current session state - Login or TermList
        await SetViewForPage();
        // render
        base.OnAppearing();

        // run startup actions, services manage own state
        await _localNotificationService.Startup();
    }
}