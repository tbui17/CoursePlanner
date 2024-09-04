using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Attributes;
using Lib.Models;
using Lib.Services.ReportService;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

[Inject]
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