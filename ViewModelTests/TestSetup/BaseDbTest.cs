using FluentAssertions.Extensions;
using Lib.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Serilog;


namespace ViewModelTests.TestSetup;

public abstract class BaseDbTest : BaseTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
        var logger = Provider.GetRequiredService<ILogger<BaseDbTest>>();
        await using var db = await factory.CreateDbContextAsync();
        logger.LogInformation("Setting up database with connection string: {ConnectionString}",
            db.Database.GetConnectionString());
        await new DbUtil(db).ResetAndSeedDb();
    }

    private static readonly ResiliencePipeline Retry = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            OnRetry = x =>
            {
                Log.Error(x.Outcome.Exception, "Error occurred. Retrying. {Context} {Outcome}", x.Context, x.Outcome);
                return ValueTask.CompletedTask;
            },
            MaxRetryAttempts = 3,
            Delay = 1.Seconds(),
            BackoffType = DelayBackoffType.Linear,
            ShouldHandle = x =>
            {
                var res = x.Outcome.Exception is IOException e && e.Message.Contains("The process cannot access the file", StringComparison.OrdinalIgnoreCase);
                return ValueTask.FromResult(res);
            }
        })
        .AddTimeout(10.Seconds())
        .Build();

    [TearDown]
    public override async Task TearDown()
    {
        await base.TearDown();

        await Retry.ExecuteAsync(async x =>
        {
            var factory = Provider.GetRequiredService<ILocalDbCtxFactory>();
            await using var db = await factory.CreateDbContextAsync(x);
            await db.Database.EnsureDeletedAsync(x);
        });
    }
}