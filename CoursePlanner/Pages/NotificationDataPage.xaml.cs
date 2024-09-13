using System.Collections;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Interfaces;
#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

namespace CoursePlanner.Pages;

public partial class NotificationDataPage : IRefreshableView<NotificationDataViewModel>,
    IViewFor<NotificationDataViewModel>
{
    private ILogger<NotificationDataPage> _logger;

    public NotificationDataPage(NotificationDataViewModel model, ILogger<NotificationDataPage> logger)
    {
        _logger = logger;
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;
        Bind();
    }

    public NotificationDataViewModel Model { get; set; }

    object? IViewFor.ViewModel
    {
        get => Model;
        set => Model = (NotificationDataViewModel)value!;
    }

    public NotificationDataViewModel ViewModel
    {
        get => Model;
        set => Model = value;
    }


    private void Bind()
    {
        this.Bind(ViewModel,
            x => x.Start,
            x => x.StartDatePickerField.Date
        );
        this.Bind(ViewModel,
            x => x.End,
            x => x.EndDatepickerField.Date
        );

        this.Bind(ViewModel,
            x => x.FilterText,
            x => x.NameTextField.Text
        );

        this.Bind(ViewModel,
            x => x.TypeFilter,
            x => x.TypeAutoCompleteField.Text
        );

        this.OneWayBind(ViewModel,
            x => x.Types,
            x => x.TypeAutoCompleteField.ItemsSource
        );

        this.OneWayBind(ViewModel,
            x => x.NotificationOptions,
            x => x.NotificationOptionPickerField.ItemsSource,
            selector: x => (IList)x
        );

        this.Bind(ViewModel,
            x => x.SelectedNotificationOptionIndex,
            x => x.NotificationOptionPickerField.SelectedIndex,
            vmToViewConverter: x => (int)x,
            viewToVmConverter: x => (ShouldNotifyIndex)x
        );

        var pageResult = this.WhenAnyValue(x => x.ViewModel.PageResult).WhereNotNull();

        pageResult
            .Select(x => x.CurrentPageData)
            .BindTo(this, x => x.NotificationItemInstance.ItemsSource);

        pageResult.Select(x => $"Page Count: {x.PageCount}")
            .BindTo(this, x => x.ItemCountLabel.Text);
    }
}