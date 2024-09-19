using CommunityToolkit.Maui.Markup;
using CoursePlanner.Templates;
using Microsoft.Extensions.Logging;
using ViewModels.Domain;
using ViewModels.Interfaces;

namespace CoursePlanner.Pages;

public partial class StatsPage : IRefreshableView<StatsViewModel>
{
    public StatsViewModel Model { get; set; }

    public StatsPage(StatsViewModel model, ILogger<StatsPage> logger, ReportCardTemplate template)
    {
        logger.LogDebug("Creating StatsPage");
        Model = model;
        InitializeComponent();
        logger.LogDebug("StatsPage Initialized");
        BindingContext = Model;
        HideSoftInputOnTapped = true;
        CollectionViewInstance
            .ItemTemplate(template.Create())
            .Bind(nameof(Model.DurationReport));
    }
}