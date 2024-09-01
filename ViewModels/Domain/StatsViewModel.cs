using CommunityToolkit.Mvvm.ComponentModel;

using Lib.Models;
using Lib.Services.ReportService;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

public partial class StatsViewModel(ReportService reportService) : ObservableObject, IRefresh
{

    [ObservableProperty]
    private AggregateDurationReport _durationReport = new();


    public async Task RefreshAsync()
    {
        var res = await reportService.GetDurationReport();
        DurationReport = (AggregateDurationReport)res;
    }
}