using Lib.Interfaces;
using Lib.Models;
using OneOf;
using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class StatsPage : IRefreshableView<StatsViewModel>
{
    public StatsViewModel Model { get; set; }

    public StatsPage(StatsViewModel model)
    {
        Model = model;
        InitializeComponent();
        BindingContext = Model;
        HideSoftInputOnTapped = true;
        Model.PropertyChanged += (_, x) =>
        {
            if (x.PropertyName is not nameof(model.DurationReport)) return;
            ReportsLayout.Children.Clear();
            ReportsLayout.Children.Add(CreateContent());
        };
    }

    private IView CreateContent()
    {
        var border = new Border
        {
            Stroke = Palette.Primary,
            StrokeThickness = 2,
            Margin = new Thickness(20),
            Content = new ReportViewFactory(Model.DurationReport).CreateTableView()
        };

        return border;
    }
}

file class ReportViewFactory(AggregateDurationReport report)
{

    public TableView CreateTableView()
    {
        var tableSections = new[] { report }
            .Concat(report.SubReports.Select(x => x.Value))
            .Select(CreateTableSection);

        return new TableView
        {
            Intent = TableIntent.Data,
            Root = { tableSections },
        };
    }

    private static TableSection CreateTableSection(IDurationReport report)
    {
        var section = new TableSection
        {
            Cell("Total Time", report.TotalTime),
            Cell("Completed Time", report.CompletedTime),
            Cell("Remaining Time", report.RemainingTime),
            Cell("Average Duration", report.AverageDuration),
            Cell("Min Date", report.MinDate),
            Cell("Max Date", report.MaxDate),
            Cell("Total Items", report.TotalItems),
            Cell("Completed Items", report.CompletedItems),
            Cell("Remaining Items", report.RemainingItems),
            Cell("Percent Complete", report.PercentComplete),
            Cell("Percent Remaining", report.PercentRemaining)
        };
        section.Title = GetTitle();

        return section;


        string GetTitle() => report switch
        {
            DurationReport x => x.Type.Name,
            AggregateDurationReport => "Aggregate Report",
            _ => "Report"
        };
    }

    private static TextCell Cell(string text, OneOf<DateTime, int, TimeSpan, double> detail) => new()
    {
        Text = text,
        Detail = detail.Match(
            date => date.ToString("MM/dd/yyyy"),
            int1 => int1.ToString(),
            timeSpan => timeSpan.TotalDays.ToString("N0"),
            double1 => double1.ToString("F")
        ),
        TextColor = Palette.Secondary,
        DetailColor = Palette.Tertiary
    };
}

file static class Palette
{
    public static Color Primary => Get(nameof(Primary));
    public static Color Secondary => Get(nameof(Secondary));
    public static Color Tertiary => Get(nameof(Tertiary));
    private static Color Get(string key) => Application.Current?.Resources[key] is Color s ? s : Colors.MidnightBlue;
}