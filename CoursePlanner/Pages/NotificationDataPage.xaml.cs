using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class NotificationDataPage : IVmView<NotificationDataViewModel>
{
    public NotificationDataPage(NotificationDataViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;
    }

    public NotificationDataViewModel Model { get; set; }
}