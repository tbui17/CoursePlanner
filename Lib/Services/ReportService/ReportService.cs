using Lib.Interfaces;
using Lib.Services.MultiDbContext;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.ReportService;

public class ReportService(MultiLocalDbContextFactory dbFactory)
{
    public async Task<IDurationReport> GetDurationReport()
    {
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeEntity>();
        var reports = await db.Query(async q =>
        {
            var res = await q.ToListAsync();
            return new DurationReportFactory { Entities = res, Date = now }.Create();
        });

        return new AggregateDurationReportFactory{Reports = reports}.Create();
    }
}