using ViewModels.Domain;
using ViewModels.Domain;

namespace CoursePlanner.Pages;

public partial class NotificationDataPage
{
    public NotificationDataPage(NotificationDataViewModel model)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
    }

    public NotificationDataViewModel Model { get; set; }
}