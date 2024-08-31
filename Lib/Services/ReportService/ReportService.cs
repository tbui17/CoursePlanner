using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using Microsoft.EntityFrameworkCore;

namespace Lib.Services.ReportService;

public class ReportService(MultiLocalDbContextFactory dbFactory)
{
    public async Task<IDurationReport> GetDurationReport()
    {
        return new AggregateDurationReportFactory { Reports = await GetReports() }.Create();

        async Task<IList<DurationReport>> GetReports()
        {
            var now = DateTime.Now.Date;
            await using var db = await dbFactory.CreateAsync<IDateTimeEntity>();
            return await db.Query(q => Task.Run(async () =>
            {
                var res = await q.AsNoTracking().ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now };
                var results = fac.Create();
                res.Clear();
                return results;
            }));
        }
    }
}