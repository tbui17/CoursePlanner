using System.Collections;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
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
        //  dates
        StartDatePickerField.DatePickerView
            .ToDateSelectedObservable()
            .Select(x => x.EventArgs)
            .Subscribe(ViewModel.ChangeStartDate);


        this.Bind(
            ViewModel,
            x => x.Start,
            x => x.StartDatePickerField.Date
        );

        EndDatePickerField.DatePickerView
            .ToDateSelectedObservable()
            .Select(x => x.EventArgs)
            .Subscribe(ViewModel.ChangeEndDate);

        this.Bind(
            ViewModel,
            x => x.End,
            x => x.EndDatePickerField.Date
        );


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

        var pageResult = this.WhenAnyValue(x => x.ViewModel.PageResult);

        this.OneWayBind(ViewModel,
            x => x.NextCommand,
            x => x.NextButton.Command
        );
        this.OneWayBind(ViewModel,
            x => x.PreviousCommand,
            x => x.PreviousButton.Command
        );

        pageResult
            .Select(x => x.CurrentPageData)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => NotificationItemInstance.ItemsSource = x);

        pageResult
            .Select(x => $"Page: {x.CurrentPage} of {x.PageCount}")
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => ItemCountLabel.Text = x);

        // command

        this.OneWayBind(ViewModel,
            x => x.ClearCommand,
            x => x.ClearButton.Command
        );
    }
}

file static class ObservableExtensions
{
    public static IObservable<EventPattern<DateChangedEventArgs>> ToDateSelectedObservable(this DatePickerView view) =>
        Observable.FromEventPattern<DateChangedEventArgs>(
            h => view.DateSelected += h,
            h => view.DateSelected -= h
        );
}