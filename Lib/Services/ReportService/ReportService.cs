using Lib.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.ReportService;

[Inject]
public class ReportService(MultiLocalDbContextFactory dbFactory)
{
    public async Task<IDurationReport> GetAggregateDurationReportData(DateTime? date = null)
    {
        var time = date ?? DateTime.Now.Date;
        return new AggregateDurationReportFactory { Reports = await GetDurationReportData(time)}.Create();
    }

    public async Task<IReadOnlyCollection<DurationReportFactory>> GetDurationReportData(DateTime? date = default)
    {
        var now = date ?? DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.QueryThreaded(async q =>
            {
                var res = await q.AsNoTracking().Select(x => new DateTimeRange{Start = x.Start, End = x.End}).ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now,Type = q.ElementType};
                return fac;
            }
        );
        return res.AsReadOnly();
    }
}