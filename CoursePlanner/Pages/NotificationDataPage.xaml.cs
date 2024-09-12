using System.Collections;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using UraniumUI.Material.Controls;
using ViewModels.Domain;
using ViewModels.Interfaces;

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

        this.Bind(ViewModel,
            x => x.Types,
            x => x.TypeAutoCompleteField.ItemsSource
        );

        this.Bind(ViewModel,
            x => x.NotificationOptions,
            x => x.NotificationOptionPickerField.ItemsSource,
            vmToViewConverter: x => (IList)x!,
            viewToVmConverter: x => (IList<string>)x
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

public class SelectedNotificationOptionIndexConverter : IBindingTypeConverter
{
    public int GetAffinityForObjects(Type fromType, Type toType)
    {
        if (fromType == typeof(int) && toType == typeof(ShouldNotifyIndex))
        {
            return 100;
        }

        if (fromType == typeof(ShouldNotifyIndex) && toType == typeof(int))
        {
            return 100;
        }

        return 0;
    }

    public bool TryConvert(
        object? from,
        Type toType,
        object? conversionHint,
        out object? result
    )
    {
        switch (from)
        {
            case int i when toType == typeof(ShouldNotifyIndex):
                result = (ShouldNotifyIndex)i;
                return true;
            case ShouldNotifyIndex s when toType == typeof(int):
                result = (int)s;
                return true;
            default:
                result = null;
                return false;
        }
    }
}