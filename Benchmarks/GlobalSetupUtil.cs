using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Benchmarks;

public class GlobalSetupUtil
{
    public ServiceProvider SetupContainer()
    {
        var services = new ServiceCollection();
        var path = Environment.GetEnvironmentVariable("DB_PATH");
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("DB_PATH environment variable not set");
        }


        services
            .AddBackendServices()
            .AddInjectables()
            .AddTransient<ReportService>()
            .AddTestDatabase(path)
            .AddLogging(x => x.ClearProviders().SetMinimumLevel(LogLevel.Critical));

        var provider = services.BuildServiceProvider();
        return provider;
    }

    public async Task SetupImpl(LocalDbCtx db)
    {
        var timer = new System.Diagnostics.Stopwatch();
        timer.Start();
        Console.WriteLine("Started db setup");

        var util = new DbUtil(db);
        Console.WriteLine($"Resetting and seeding db, elapsed: {timer.ElapsedMilliseconds}ms");
        await util.ResetAndSeedDb();

        Console.WriteLine($"Finished first db setup in {timer.ElapsedMilliseconds}ms");

        Console.WriteLine($"Starting second db setup at {timer.ElapsedMilliseconds}ms");
        var terms = CreateTerms();
        Console.WriteLine($"Finished terms in {timer.ElapsedMilliseconds}ms");
        var courses = CreateCourses();
        Console.WriteLine($"Finished courses at {timer.ElapsedMilliseconds}ms");
        var assessments = CreateAssessments();
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
    }

    private static ParallelQuery<T> Create<T>(Func<int, T> fn) => Enumerable.Range(0, 100000).AsParallel().Select(fn);

    public IEnumerable<Term> CreateTerms() => Create(i => new Term { Name = $"Test Term {i}" });
    public IEnumerable<Course> CreateCourses() => Create(i => new Course { Name = $"Test Course {i}", TermId = 1 });

    public IEnumerable<Assessment> CreateAssessments() =>
        Create(i => new Assessment { Name = $"Test Assessment {i}", CourseId = 1 });

    public IEnumerable<DateTimeRange> CreateDateTimeRanges() =>
        Create(_ => new DateTimeRange { Start = DateTime.Now, End = DateTime.Now.AddDays(1) });


    public async Task<ServiceProvider> GlobalSetup()
    {
        var provider = SetupContainer();
        await using var db = await provider.GetDb();
        Console.WriteLine($"Connection string: {db.Database.GetConnectionString()}");
        if (await db.Database.CanConnectAsync())
        {
            Console.WriteLine("Database already exists, skipping setup");
            return provider;
        }

        await SetupImpl(db);
        return provider;
    }
}