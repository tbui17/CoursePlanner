using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using Lib.Utils;
using Plainer.Maui.Controls;
using ReactiveUI;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Interfaces;

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

namespace CoursePlanner.Pages;

public partial class NotificationDataPage : IRefreshableView<NotificationDataViewModel>,
    IViewFor<NotificationDataViewModel>
{
    public NotificationDataPage(NotificationDataViewModel model)
    {
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
        // bind dates
        StartDatePickerField.DatePickerView.Thru(ToDateSelectedObservable)
            .Select(x => x.EventArgs.NewDate)
            .Subscribe(x => ViewModel.ChangeStartDateCommand.Execute(x));

        this.WhenAnyValue(x => x.ViewModel.Start)
            .BindTo(this, x => x.StartDatePickerField.Date);

        this.WhenAnyValue(x => x.ViewModel.End)
            .BindTo(this, x => x.EndDatePickerField.Date);

        EndDatePickerField.DatePickerView.Thru(ToDateSelectedObservable)
            .Select(x => x.EventArgs.NewDate)
            .Subscribe(x => ViewModel.ChangeEndDateCommand.Execute(x));


        // text filters
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

        // filter options

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

        // page result

        var pageResult = this.WhenAnyValue(x => x.ViewModel.PageResult).WhereNotNull();

        pageResult.Select(x => x.CurrentPage)
            .BindTo(this, x => x.PaginatorInstance.CurrentPage);

        pageResult
            .Select(x => x.PageCount)
            .BindTo(this, x => x.PaginatorInstance.TotalPageCount);


        pageResult
            .Select(x => x.CurrentPageData)
            .BindTo(this, x => x.NotificationItemInstance.ItemsSource);

        pageResult.Select(x => $"Page Count: {x.PageCount}")
            .BindTo(this, x => x.ItemCountLabel.Text);

        // command

        this.OneWayBind(ViewModel,
            x => x.ChangePageCommand,
            x => x.PaginatorInstance.ChangePageCommand
        );

        this.OneWayBind(ViewModel,
            x => x.ClearCommand,
            x => x.ClearButton.Command
        );
    }

    private static IObservable<EventPattern<DateChangedEventArgs>> ToDateSelectedObservable(DatePickerView view)
    {
        return Observable.FromEventPattern<DateChangedEventArgs>(
            h => view.DateSelected += h,
            h => view.DateSelected -= h
        );
    }
}