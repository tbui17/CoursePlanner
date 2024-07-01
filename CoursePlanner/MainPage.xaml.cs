﻿using CoursePlanner.Services;
using CoursePlanner.ViewModels;
using Plugin.LocalNotification;

namespace CoursePlanner;

public partial class MainPage : ContentPage
{
    public MainViewModel Model { get; set; }
    public AppService ShellModel { get; set; }

    public MainPage(MainViewModel model, AppService shellModel)
    {
        Model = model;
        ShellModel = shellModel;
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