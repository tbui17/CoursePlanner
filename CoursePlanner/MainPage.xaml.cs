﻿
using Plugin.LocalNotification;
using ViewModels.PageViewModels;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    public MainViewModel Model { get; set; }

    public MainPage(MainViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.Init();

        if (!await LocalNotificationCenter.Current.AreNotificationsEnabled())
        {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }
    }
}