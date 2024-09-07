using CoursePlanner.Views;
using Microsoft.Extensions.Logging;
using ViewModels.Domain;
using ViewModels.Services;

namespace CoursePlanner;

public partial class MainPage
{
    private readonly ILocalNotificationService _localNotificationService;

    public MainViewModel Model { get; set; }

    private readonly ISessionService _sessionService;
    private readonly IServiceProvider _provider;
    private readonly ILogger<MainPage> _logger;

    public MainPage(MainViewModel model, IServiceProvider provider, ILocalNotificationService localNotificationService,
        ISessionService sessionService, ILogger<MainPage> logger)
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

    private void SetView(IView view)
    {
        _logger.LogInformation("Setting view {Type}", view);
        MainLayout.Clear();
        MainLayout.Add(view);
    }

    protected override async void OnAppearing()
    {

        _logger.LogInformation("User sign in status: {IsLoggedIn} {Is}", _sessionService.IsLoggedIn,_sessionService.User?.Username);
        if (_sessionService.IsLoggedIn)
        {

            var view = _provider.GetRequiredService<TermListView>();
            await view.Model.RefreshAsync();
            SetView(view);
        }
        else
        {

            SetView(_provider.GetRequiredService<LoginView>());
        }

        base.OnAppearing();

        await _localNotificationService.RequestNotificationPermissions();

    }
}