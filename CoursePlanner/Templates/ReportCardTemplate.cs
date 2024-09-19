using System.Diagnostics;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Markup.LeftToRight;
using Humanizer;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using ViewModels.Converters;
using ViewModels.Services;

namespace CoursePlanner.Templates;

[Inject]
public class ReportCardTemplate(IAppService app)
{
    public DataTemplate Create()
    {
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
                        new Label().Bind(nameof(DurationReportData.Title)),
                        CreateCard()
                    }
                }
            }
        );
    }

    VerticalStackLayout CreateCard()
    {
        var stack = new VerticalStackLayout
        {
            Spacing = 5,
        };

        var labels = DurationReportData
            .GetLabelProperties()
            .Select(x => new Label().Bind(
                    path: x.Name,
                    stringFormat: $"{x.Name.Humanize(LetterCasing.Title)}: {{0}}"
                )
            )
            .ToList();


        foreach (var label in labels)
        {
            stack.Add(label);
        }

        return stack;
    }
}