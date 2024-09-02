using Lib.Interfaces;
using Lib.Models;
using OneOf;
using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

using DetailValue = OneOf<DateTime, int, TimeSpan, double>;

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
        var tables = new ReportViewFactory(Model.DurationReport).CreateTableView();
        var border = new Border
        {
            Stroke = Palette.Primary,
            StrokeThickness = 2,
            Margin = new Thickness(20),
            Content = tables
        };

        return border;
    }
}

file class ReportViewFactory(AggregateDurationReport report)
{
    private const string AggregateReport = "Aggregate Report";

    public TableView CreateTableView()
    {
        var tableSections = new[] { report }
            .AsParallel()
            .Concat(report.SubReports.Select(x => x.Value))
            .Select(CreateTableSection)
            .OrderBy(x => x.Title, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(x => x.Title is AggregateReport);

        return new TableView
        {
            Intent = TableIntent.Data,
            Root = { tableSections },
        };
    }

    private static TableSection CreateTableSection(IDurationReport report)
    {
        var f = new CellFactory();
        var section = new TableSection
        {
            f.Create("Time Progress", "{0}/{1} days", report.CompletedTime, report.TotalTime),
            f.Create("Time Remaining", "{0} days", report.RemainingTime),
            f.Create("Average Duration", "{0} days", report.AverageDuration),
            f.Create("Min Date", "{0}", report.MinDate),
            f.Create("Max Date", "{0}", report.MaxDate),
            f.Create("Item Progress", "{0}/{1}", report.CompletedItems, report.TotalItems),
            f.Create("Item Remaining", "{0}", report.RemainingItems),
            f.Create("Percent Items Complete", "{0}%", report.PercentComplete),
            f.Create("Percent Items Remaining", "{0}%", report.PercentRemaining),
        };

        section.Title = GetTitle();
        section.TextColor = Palette.Primary;

        return section;


        string GetTitle() => report switch
        {
            DurationReport x => x.Type.Name,
            AggregateDurationReport => AggregateReport,
            _ => "Report"
        };
    }
}

file class CellFactory
{
    public TextCell Create(string text, string detail, params DetailValue[] args) => new()
    {
        Text = text,
        Detail = string.Format(detail, args.Select(Convert).Cast<object>().ToArray()),
        TextColor = Palette.Secondary,
        DetailColor = Palette.BodyText
    };

    private static string Convert(DetailValue detail) => detail.Match(
        date => date.ToString("MM/dd/yyyy"),
        int1 => int1.ToString(),
        timeSpan => timeSpan.TotalDays.ToString("N0"),
        double1 => double1.ToString("F")
    );
}

file static class Palette
{
    public static Color Primary => Get(nameof(Primary));
    public static Color Secondary => Get(nameof(Secondary));
    public static Color Tertiary => Get(nameof(Tertiary));
    public static Color BodyText => Application.Current?.RequestedTheme is AppTheme.Dark ? Colors.White : Colors.Black;
    private static Color Get(string key) => Application.Current?.Resources[key] as Color ?? Colors.MidnightBlue;
}