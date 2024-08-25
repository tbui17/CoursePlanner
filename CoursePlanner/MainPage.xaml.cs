﻿using CoursePlanner.Views;
using ViewModels.PageViewModels;
using ViewModels.Services;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    private readonly ILocalNotificationService _localNotificationService;

    public MainViewModel Model { get; set; }

    private readonly ISessionService _sessionService;
    private readonly IServiceProvider _provider;

    public MainPage(MainViewModel model, IServiceProvider provider, ILocalNotificationService localNotificationService,
        ISessionService sessionService)
    {
        _sessionService = sessionService;
        _provider = provider;
        _localNotificationService = localNotificationService;
        Model = model;
        InitializeComponent();
        BindingContext = this;
    }

    private void SetView(IView view)
    {
        MainLayout.Clear();
        MainLayout.Add(view);
    }

    protected override async void OnAppearing()
    {
        if (_sessionService.IsLoggedIn)
        {
            var view = _provider.GetRequiredService<TermListView>();
            await view.Model.Refresh();
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