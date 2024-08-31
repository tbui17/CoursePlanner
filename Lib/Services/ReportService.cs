using Lib.Services.MultiDbContext;

namespace Lib.Services;

public class ReportService(MultiLocalDbContextFactory dbFactory)
{


    public async Task GetStats()
    {
        var today = DateTime.Now.Date;

    }
}