using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class InstructorFormPage : ContentPage
{
    public InstructorFormPage(InstructorFormViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
    }

    public InstructorFormViewModel Model { get; set; }
}