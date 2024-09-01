using CommunityToolkit.Mvvm.ComponentModel;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using ViewModels.Interfaces;

namespace ViewModels.Domain;

public partial class StatsViewModel(ReportService reportService) : ObservableObject, IRefresh
{

    [ObservableProperty]
    private IDurationReport _durationReport = new AggregateDurationReport();


    public async Task RefreshAsync()
    {
        DurationReport = await reportService.GetDurationReport();
    }
}