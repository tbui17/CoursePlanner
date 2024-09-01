using BaseTestSetup;
using BenchmarkDotNet.Attributes;
using Lib;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.MultiDbContext;
using Lib.Services.ReportService;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Benchmarks;

public class ReportBenchmark
{
    private IServiceProvider Provider { get; set; } = null!;


    private void SetupContainer()
    {


        var services = new ServiceCollection();
        var path = Environment.GetEnvironmentVariable("DB_PATH");
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("DB_PATH environment variable not set");
        }
        services
            .AddBackendServices()
            .AddTransient<ReportServiceBenchmark>()
            .AddTestDatabase(path)
            .AddLogging(x => x.ClearProviders().SetMinimumLevel(LogLevel.Critical));

        var provider = services.BuildServiceProvider();
        Provider = provider;
    }

    private async Task SetupImpl(LocalDbCtx db)
    {

        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        Console.WriteLine("Started db setup");

        var util = new DbUtil(db);
        Console.WriteLine($"Resetting and seeding db, elapsed: {timer.ElapsedMilliseconds}ms");
        await util.ResetAndSeedDb();

        Console.WriteLine($"Finished first db setup in {timer.ElapsedMilliseconds}ms");

        Console.WriteLine($"Starting second db setup at {timer.ElapsedMilliseconds}ms");
        var terms = Create(i => new Term { Name = $"Test Term {i}" });
        Console.WriteLine($"Finished terms in {timer.ElapsedMilliseconds}ms");
        var courses = Create(i => new Course { Name = $"Test Course {i}", TermId = 1 });
        Console.WriteLine($"Finished courses at {timer.ElapsedMilliseconds}ms");
        var assessments = Create(i => new Assessment { Name = $"Test Assessment {i}", CourseId = 1 });
        Console.WriteLine($"Finished assessments at {timer.ElapsedMilliseconds}ms");

        Console.WriteLine($"Adding entities to db at {timer.ElapsedMilliseconds}ms");
        db.Terms.AddRange(terms);
        Console.WriteLine($"Added terms at {timer.ElapsedMilliseconds}ms");
        db.Courses.AddRange(courses);
        Console.WriteLine($"Added courses at {timer.ElapsedMilliseconds}ms");
        db.Assessments.AddRange(assessments);
        Console.WriteLine($"Added assessments at {timer.ElapsedMilliseconds}ms");
        await db.SaveChangesAsync();
        Console.WriteLine($"Saved changes at {timer.ElapsedMilliseconds}ms");

        timer.Stop();
        Console.WriteLine($"Finished all db setup in {timer.ElapsedMilliseconds}ms");

        return;

        ParallelQuery<T> Create<T>(Func<int, T> fn) => Enumerable.Range(0, 100000).AsParallel().Select(fn);
    }

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        SetupContainer();
        await using var db = await GetDb();
        Console.WriteLine($"Connection string: {db.Database.GetConnectionString()}");
        if (await db.Database.CanConnectAsync())
        {
            Console.WriteLine("Database already exists, skipping setup");
            return;
        }
        await SetupImpl(db);
    }

    private async Task<LocalDbCtx> GetDb()
    {
        return await Provider.GetRequiredService<IDbContextFactory<LocalDbCtx>>().CreateDbContextAsync();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> Impl1()
    {
        var report = Provider.GetRequiredService<ReportServiceBenchmark>();
        var res = await report.GetDurationReport();
        return res.TotalItems;
    }

    [Benchmark]
    public async Task<int> Impl2()
    {
        var report = Provider.GetRequiredService<ReportServiceBenchmark>();
        var res = await report.GetDurationReport2();
        return res.TotalItems;
    }

    [Benchmark]
    public async Task<int> Impl3()
    {
        var report = Provider.GetRequiredService<ReportServiceBenchmark>();
        var res = await report.GetDurationReport3();
        return res.TotalItems;
    }
}


public class ReportServiceBenchmark(MultiLocalDbContextFactory dbFactory)
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
                var fac = new DurationReportFactoryBenchmark { Entities = res, Date = now };
                var results = fac.Create();
                res.Clear();
                return results;
            }));
        }
    }

    public async Task<IDurationReport> GetDurationReport2()
    {
        return new AggregateDurationReportFactory { Reports = await GetReports() }.Create();

        async Task<IList<DurationReport>> GetReports()
        {
            var now = DateTime.Now.Date;
            await using var db = await dbFactory.CreateAsync<IDateTimeEntity>();

            return await db.Query(async q =>
            {
                var res = await q.AsNoTracking().ToListAsync();
                var fac = new DurationReportFactoryBenchmark { Entities = res, Date = now };
                var results = fac.Create();
                res.Clear();
                return results;
            });
        }
    }

    public async Task<IDurationReport> GetDurationReport3()
    {
        return new AggregateDurationReportFactory { Reports = await GetReports() }.Create();

        async Task<IList<DurationReport>> GetReports()
        {
            var now = DateTime.Now.Date;
            await using var db = await dbFactory.CreateAsync<IDateTimeEntity>();
            return await db.Query(q => Task.Run(async () =>
            {
                var res = await q.AsNoTracking().ToListAsync();
                var fac = new DurationReportFactoryBenchmark { Entities = res, Date = now };
                var results = await fac.CreateAsync();
                res.Clear();
                return results;
            }));
        }
    }
}


public class DurationReportFactoryBenchmark
{
    public IList<IDateTimeEntity> Entities { get; set; } = [];
    public DateTime Date { get; set; } = DateTime.Now.Date;

    public int TotalItems() => Entities.Count;

    public int CompletedItems() => Entities.Count(x => x.End < Date);

    public TimeSpan TotalTime() => MaxDate() - MinDate();

    public TimeSpan CompletedTime()
    {
        var beforeDate = BeforeDate();
        return beforeDate.MaxOrDefault(x => x.End)
               -
               beforeDate.MinOrDefault(x => x.Start);
    }

    private List<IDateTimeEntity> BeforeDate() => Entities.Where(x => x.End < Date).ToList();


    public TimeSpan RemainingTime() => new[] { MaxDate() - Date, TimeSpan.Zero }.Max();

    public TimeSpan AverageDuration() =>
        Entities.AverageOrDefault(x => x.Duration());

    public Type Type() => Entities.FirstOrDefault()?.GetType() ?? typeof(IDateTimeEntity);

    public DateTime MinDate() => Entities.MinOrDefault(x => x.Start);
    public DateTime MaxDate() => Entities.MaxOrDefault(x => x.End);

    public DurationReport Create() => new()
    {
        TotalTime = TotalTime(),
        CompletedTime = CompletedTime(),
        RemainingTime = RemainingTime(),
        AverageDuration = AverageDuration(),
        TotalItems = TotalItems(),
        CompletedItems = CompletedItems(),
        Type = Type(),
        MinDate = MinDate(),
        MaxDate = MaxDate()
    };

    public async Task<DurationReport> CreateAsync()
    {
        var totalTimeTask = Task.Run(TotalTime);
        var completedTimeTask = Task.Run(CompletedTime);
        var remainingTimeTask = Task.Run(RemainingTime);
        var averageDurationTask = Task.Run(AverageDuration);
        var totalItemsTask = Task.Run(TotalItems);
        var completedItemsTask = Task.Run(CompletedItems);
        var typeTask = Task.Run(Type);
        var minDateTask = Task.Run(MinDate);
        var maxDateTask = Task.Run(MaxDate);

        await Task.WhenAll(totalTimeTask, completedTimeTask, remainingTimeTask, averageDurationTask, totalItemsTask, completedItemsTask, typeTask, minDateTask, maxDateTask);

        return new DurationReport
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