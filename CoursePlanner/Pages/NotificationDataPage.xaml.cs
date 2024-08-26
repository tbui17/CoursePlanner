using CommunityToolkit.Maui.Markup;
using CoursePlanner.Controls;
using ViewModels.Interfaces;
using ViewModels.PageViewModels;

namespace CoursePlanner.Pages;

public partial class NotificationDataPage : IPageModel<NotificationDataViewModel>
{
    public NotificationDataPage(NotificationDataViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = model;

        var header = new VerticalStackLayout
        {
            Margin = new Thickness(10),
            Spacing = 10,
            Children =
            {
                new AutoGrid
                {
                    Children =
                    {
                        new Label().Text("Month"),
                        new DatePicker().Bind(DatePicker.DateProperty, nameof(model.MonthDate)),
                        new Label().Text("Filter"),
                        new Entry().Bind(Entry.TextProperty, nameof(model.FilterText)),
                    }
                }
            }

        };

        MainLayout.Insert(0,header);


    }
    public NotificationDataViewModel Model { get; set; }
}