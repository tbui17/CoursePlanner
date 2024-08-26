﻿using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class DetailedCoursePage : IRefreshableView<DetailedCourseViewModel>
{
    public DetailedCourseViewModel Model { get; }

    public DetailedCoursePage(DetailedCourseViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }
}