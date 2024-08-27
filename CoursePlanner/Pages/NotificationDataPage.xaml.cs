using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class NotificationDataPage
{
    public NotificationDataPage(NotificationDataViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public NotificationDataViewModel Model { get; set; }
}