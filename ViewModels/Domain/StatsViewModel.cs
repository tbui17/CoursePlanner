using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using Microsoft.Extensions.Logging;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

[Inject]
public partial class StatsViewModel(ReportService reportService, ILogger<StatsViewModel> logger)
    : ObservableObject, IRefresh
{
    [ObservableProperty]
    private ObservableCollection<DurationReportData> _durationReport = [];


    public async Task RefreshAsync()
    {
        var res = await reportService.GetAggregateDurationReportData();
        DurationReport = DurationReportData.FromDurationReport(res).ToObservableCollection();
    }
}