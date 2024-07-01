﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class EditAssessmentPage : ContentPage
{
    public EditAssessmentPage(EditAssessmentViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = Model;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Model.RefreshAsync();
    }

    public EditAssessmentViewModel Model { get; set; }
    
    
}