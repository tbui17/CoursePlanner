using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
using Lib.Utils;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;

namespace BaseTestSetup;

using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public static class BaseTestConfig
{
    public static IServiceCollection AddLogger(this IServiceCollection services, bool useGlobalLogger = true)
    {
        return new LogConfigurationTestUseCase()
            .SetMinimumLogLevel(x => x.Debug())
            .AddDefaultSinks()
            .AddDefaultEnrichments()
            .AddFileSink()
            .AddDatabaseInitializationFilters()
            .AddSeq()
            .Finalize(services, useGlobalLogger);
    }

    public static IServiceCollection AddTestDatabase(this IServiceCollection services, string? fileName = null)
    {
        var name = fileName ?? Guid.NewGuid().ToString();
        return services.AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={name}")
            )
            .AddDbContextFactory<LocalDbCtx>();
    }

    public static async Task GlobalTearDown()
    {
        await Log.CloseAndFlushAsync();
    }
}

file class LogConfigurationTestUseCase
{
    public LoggerConfiguration Configuration { get; init; } = new();

    public LogConfigurationTestUseCase SetMinimumLogLevel(Action<LoggerMinimumLevelConfiguration> setter)
    {
        setter(Configuration.MinimumLevel);
        return this;
    }

    public LogConfigurationTestUseCase AddDefaultSinks()
    {
        const string template = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        Configuration
            .WriteTo.Console(LogEventLevel.Information, template)
            .WriteTo.Debug(LogEventLevel.Debug, template);
        return this;
    }

    public LogConfigurationTestUseCase AddDefaultEnrichments()
    {
        Configuration
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("FriendlyApplicationName", AppDomain.CurrentDomain.FriendlyName)
            .Enrich.With(new StackTraceEnricher());
        AppDataRecord.Parse(AppDomain.CurrentDomain.BaseDirectory).Enrich(Configuration);
        return this;
    }

    public LogConfigurationTestUseCase AddFileSink(string? path = default)
    {
        var logPath = path ?? Path.Combine(Path.GetTempPath(), "TestLogs", "logs.txt");
        Configuration.WriteTo.File(
            formatter: new JsonFormatter(),
            path: logPath,
            restrictedToMinimumLevel: LogEventLevel.Information,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 3,
            rollOnFileSizeLimit: true,
            fileSizeLimitBytes: (long)(300 * Math.Pow(1024, 2)),
            shared: true,
            encoding: Encoding.UTF8,
            retainedFileTimeLimit: TimeSpan.FromDays(1)
        );

        return this;
    }

    public LogConfigurationTestUseCase AddDatabaseInitializationFilters()
    {
        var shouldExcludeMessage = new Func<string, bool>[]
            {
                x => x.Contains("CREATE"),
                x => x.Contains("PRAGMA"),
            }
            .ToAnyPredicate();
        Configuration.Filter.ByExcluding(x => x.RenderMessage().Thru(shouldExcludeMessage));
        return this;
    }

    public LogConfigurationTestUseCase AddSeq()
    {
        var url = Environment.GetEnvironmentVariable("SEQ_URL");
        var apiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        if (url is not null && apiKey is not null)
        {
            Configuration.WriteTo.Seq(url, LogEventLevel.Information, apiKey: apiKey);
        }

        return this;
    }

    public IServiceCollection Finalize(IServiceCollection services, bool useGlobalLogger = true)
    {
        Logger? logger = null;
        if (useGlobalLogger)
        {
            Log.Logger = Configuration.CreateLogger();
        }
        else
        {
            logger = Configuration.CreateLogger();
        }
        return services
            .AddSerilog(logger, dispose: true)
            .AddLogging();
    }
}

file class StackTraceEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        switch (logEvent)
        {
            case { Exception: null, Level: < LogEventLevel.Error }:
                return;
            case { Exception: { StackTrace.Length: > 0 } stack }:
                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(StackTrace), stack, true));
                return;
            default:

                logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty(nameof(StackTrace), CreateStackRecords(),
                    true));
                break;
        }
    }

    private static object CreateStackRecords() =>
        new StackTrace()
            .GetFrames()
            .Select(x =>
            {
                var method = x.GetMethod() as MethodInfo;
                var module = method?.Module;
                var declaringType = method?.DeclaringType;

                var methodData = new
                {
                    method?.Name,
                    Module = module?.Name,
                    DeclaringType = new
                    {
                        declaringType?.Name,
                        declaringType?.FullName,
                        Assembly = declaringType?.Assembly.GetName().Name,
                        Module = module?.Name
                    },
                    Parameters = method?.GetParameters()
                        .Select(p => new { p.Name, ParameterType = p.ParameterType.Name })
                        .ToArray(),
                    ReturnParameter = method?.ReturnParameter.ToString(),
                };

                return new
                {
                    MethodName = method?.Name,
                    Method = methodData,
                    Offset = x.GetNativeOffset(),
                    File = x.GetFileName(),
                    Line = x.GetFileLineNumber(),
                    Column = x.GetFileColumnNumber()
                };
            })
            .ToArray();
}

[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
file record AppDataRecord
{
    public string ProjectName { get; set; } = "";
    public string Bin { get; set; } = "";
    public string Deployment { get; set; } = "";
    public string NetVersion { get; set; } = "";

    public static AppDataRecord Parse(string path)
    {
        var r = new AppDataRecord();
        List<Action<string>> setters =
        [
            s => r.NetVersion = s,
            s => r.Deployment = s,
            s => r.Bin = s,
            s => r.ProjectName = s
        ];

        var strings = path.Split("\\").Reverse().Where(x => !string.IsNullOrWhiteSpace(x));
        foreach (var (value, setter) in strings.Zip(setters))
        {
            setter(value);
        }

        return r;
    }

    private static ImmutableArray<PropertyPairFactory> PropertyGetters =>
    [
        ..typeof(AppDataRecord)
            .GetProperties()
            .Select(PropertyPairFactory (x) => record => (x.Name, x.GetValue(record)!))
    ];

    public void Enrich(LoggerConfiguration conf)
    {
        foreach (var (name, val) in PropertyGetters.Select(x => x(this)))
        {
            conf.Enrich.WithProperty(name, val);
        }
    }

    private delegate (string, object) PropertyPairFactory(AppDataRecord record);
}