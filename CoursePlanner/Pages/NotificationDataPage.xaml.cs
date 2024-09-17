using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;
using Lib.Interfaces;
using Lib.Models;
using Plainer.Maui.Controls;
using ReactiveUI;
using ViewModels.Converters;
using ViewModels.Domain.NotificationDataViewModel;
using ViewModels.Interfaces;
using ViewModels.Services;

#pragma warning disable CS8767 // Nullability of reference types in type of parameter doesn't match implicitly implemented member (possibly because of nullability attributes).

namespace CoursePlanner.Pages;

public partial class NotificationDataPage : IRefreshableView<NotificationDataViewModel>,
    IViewFor<NotificationDataViewModel>
{

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

    public NotificationDataPage(NotificationDataViewModel model, INavigationService navigationService)
    {
        Model = model;
        InitializeComponent();
        HideSoftInputOnTapped = true;
        BindingContext = model;

        var factory = new NotificationCardTemplateFactory(navigationService);
        NotificationItemInstance.ItemTemplate(factory.CreateTemplate());
        Bind();
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

file class NotificationCardTemplateFactory(INavigationService navigationService)
{
    private readonly Command<INotification> _cmd = new(entity => _ = entity switch
        {
            Course x => navigationService.GoToDetailedCoursesPageAsync(x.Id),
            Assessment x => navigationService.GoToAssessmentDetailsPageAsync(x.CourseId),
            _ => throw new ArgumentOutOfRangeException(nameof(entity), entity, "Unknown item type")
        }
    );

    public DataTemplate CreateTemplate() =>
        new(() => new Border
            {
                Content = new VerticalStackLayout
                {
                    Resources =
                        new ResourceDictionary
                        {
                            new Style(typeof(Label))
                            {
                                BasedOn = Application.Current!.Resources["BaseLabel"] as Style,
                                Setters =
                                {
                                    new Setter
                                    {
                                        Property = View.MarginProperty, Value = new Thickness(0, 0, 0, 5)
                                    },
                                    new Setter
                                    {
                                        Property = Label.FontAttributesProperty, Value = FontAttributes.Bold
                                    },
                                }
                            }
                        },
                    Spacing = 5,
                    Children =
                    {
                        new Label().Bind(nameof(INotification.Name), stringFormat: "Name: {0}"),
                        new Label().Bind(nameof(INotification.Start), stringFormat: "Start: {0}"),
                        new Label().Bind(nameof(INotification.End), stringFormat: "End: {0}"),
                        new Label().Bind(nameof(INotification.ShouldNotify), stringFormat: "Notifications: {0}"),
                        new Label().Bind(stringFormat: "Type: {0}", converter: new TypeToStringConverter()),
                        new Button { Command = _cmd, Text = "Details" }.Bind(Button.CommandParameterProperty).Left(),
                    }
                }
            }
        );
}