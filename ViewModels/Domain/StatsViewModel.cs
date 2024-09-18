using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Attributes;
using Lib.Models;
using Lib.Services.ReportService;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

[Inject]
public partial class StatsViewModel(ReportService reportService, ILogger<StatsViewModel> logger ) : ObservableObject, IRefresh
{

    [ObservableProperty]
    private AggregateDurationReport _durationReport = new();


    public async Task RefreshAsync()
    {
        logger.LogDebug("RefreshAsync triggered");
        var res = await reportService.GetAggregateDurationReportData();
        logger.LogDebug("{DurationReport}", res);
        DurationReport = (AggregateDurationReport)res;
    }
}