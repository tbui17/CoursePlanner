using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;
using Lib.Attributes;
using Lib.Interfaces;
using ViewModels.Converters;
using ViewModels.Services;

namespace CoursePlanner.Templates;

[Inject]
public class NotificationCardTemplate(INavigationService navigationService, IAppService app)
{
    public DataTemplate Create()
    {
        Command<INotification> command = new(x => navigationService.GoToNotificationDetailsPage(x));
        return new DataTemplate(() => new Border
            {
                Content = new VerticalStackLayout
                {
                    Resources = new()
                    {
                        new Style<Label>()
                            .BasedOn(app.GetStyle("BaseLabel"))
                            .Add(View.MarginProperty, new Thickness(0, 0, 0, 5))
                            .Add(Label.FontAttributesProperty, FontAttributes.Bold)
                    },
                    Spacing = 5,
                    Children =
                    {
                        new Label().Bind(nameof(INotification.Name),
                            stringFormat: "Name: {0}"
                        ),
                        new Label().Bind(nameof(INotification.Start),
                            stringFormat: "Start: {0:MM/dd/yyyy}"
                        ),
                        new Label().Bind(nameof(INotification.End), stringFormat: "End: {0:MM/dd/yyyy}"),
                        new Label().Bind(nameof(INotification.ShouldNotify), stringFormat: "Notifications: {0}"),
                        new Label().Bind(stringFormat: "Type: {0}", converter: new TypeToStringConverter()),
                        new Button { Command = command, Text = "Details" }.Bind(Button.CommandParameterProperty).Left(),
                    }
                }
            }
        );
    }
}