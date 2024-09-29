using BenchmarkDotNet.Attributes;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using Lib.Services.ReportService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks;

[MemoryDiagnoser(false)]
public class ReportBenchmark
{
    private IServiceProvider Provider { get; set; } = null!;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Provider = await new GlobalSetupUtil().GlobalSetup();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> NoMultiThreading()
    {
        var dbFactory = Provider.GetRequiredService<MultiLocalDbContextFactory>();
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.Query(async q =>
            {
                var res = await q
                    .AsNoTracking()
                    .Select(x => new DateTimeRange { Start = x.Start, End = x.End })
                    .ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now, Type = q.ElementType };

                var data = fac.ToData();
                return data;
            }
        );
        return res.Count;
    }

    [Benchmark]
    public async Task<int> MultithreadingFetch()
    {
        var dbFactory = Provider.GetRequiredService<MultiLocalDbContextFactory>();
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.QueryThreaded(async q =>
            {
                var res = await q
                    .AsNoTracking()
                    .Select(x => new DateTimeRange { Start = x.Start, End = x.End })
                    .ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now, Type = q.ElementType };
                return fac;
            }
        );

        return res.Count;
    }

    [Benchmark]
    public async Task<int> MultithreadingFetchNoMultiThreadingProcessing()
    {
        var dbFactory = Provider.GetRequiredService<MultiLocalDbContextFactory>();
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.QueryThreaded(async q =>
            {
                var res = await q
                    .AsNoTracking()
                    .Select(x => new DateTimeRange { Start = x.Start, End = x.End })
                    .ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now, Type = q.ElementType };
                return fac;
            }
        );

        return res.Select(x => x.ToData()).Count();
    }


    [Benchmark]
    public async Task<int> Multithreading()
    {
        var dbFactory = Provider.GetRequiredService<MultiLocalDbContextFactory>();
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.QueryThreaded(async q =>
            {
                var res = await q
                    .AsNoTracking()
                    .Select(x => new DateTimeRange { Start = x.Start, End = x.End })
                    .ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now, Type = q.ElementType };
                var data = fac.ToData();
                return data;
            }
        );
        return res.Count;
    }

    [Benchmark]
    public async Task<int> MultithreadingDeep()
    {
        var dbFactory = Provider.GetRequiredService<MultiLocalDbContextFactory>();
        var now = DateTime.Now.Date;
        await using var db = await dbFactory.CreateAsync<IDateTimeRange>();

        var res = await db.QueryThreaded(async q =>
            {
                var res = await q
                    .AsNoTracking()
                    .Select(x => new DateTimeRange { Start = x.Start, End = x.End })
                    .ToListAsync();
                var fac = new DurationReportFactory { Entities = res, Date = now, Type = q.ElementType };

                var impl = new ParallelImpl() { Factory = fac };
                var data = await impl.Create();
                return data;
            }
        );
        return res.Count;
    }
}

public class ParallelImpl
{
    public required IDurationReportFactory Factory { get; init; }

    public async Task<DurationReport> Create()
    {
        var totalTimeTask = Task.Run(() => Factory.TotalTime);
        var completedTimeTask = Task.Run(() => Factory.CompletedTime);
        var remainingTimeTask = Task.Run(() => Factory.RemainingTime);
        var averageDurationTask = Task.Run(() => Factory.AverageDuration);
        var totalItemsTask = Task.Run(() => Factory.TotalItems);
        var completedItemsTask = Task.Run(() => Factory.CompletedItems);
        var typeTask = Task.Run(() => Factory.Type);
        var minDateTask = Task.Run(() => Factory.MinDate);
        var maxDateTask = Task.Run(() => Factory.MaxDate);

        await Task.WhenAll(
            totalTimeTask,
            completedTimeTask,
            remainingTimeTask,
            averageDurationTask,
            totalItemsTask,
            completedItemsTask,
            typeTask,
            minDateTask,
            maxDateTask
        );

        return new DurationReport()
        {
            TotalTime = totalTimeTask.Result,
            CompletedTime = completedTimeTask.Result,
            RemainingTime = remainingTimeTask.Result,
            AverageDuration = averageDurationTask.Result,
            TotalItems = totalItemsTask.Result,
            CompletedItems = completedItemsTask.Result,
            Type = typeTask.Result,
            MinDate = minDateTask.Result,
            MaxDate = maxDateTask.Result
        };
    }
}