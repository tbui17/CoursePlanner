using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoursePlanner.ViewModels;

namespace CoursePlanner;

public partial class EditNotePage : ContentPage
{
    public EditNotePage(EditNoteViewModel model)
    {
        InitializeComponent();
        Model = model;
        HideSoftInputOnTapped = true;
    }

    public EditNoteViewModel Model { get; set; }
}