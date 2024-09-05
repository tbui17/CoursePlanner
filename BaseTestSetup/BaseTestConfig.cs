using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
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
    public static IServiceCollection AddLogger(this IServiceCollection services, Action<LoggerConfiguration> setter)
    {
        const string template = "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        var conf = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("FriendlyApplicationName", AppDomain.CurrentDomain.FriendlyName)
            .Enrich.With(new StackTraceEnricher())
            .WriteTo.Console(LogEventLevel.Information, template)
            .WriteTo.Debug(LogEventLevel.Debug, template);
        AddFileLogging(conf);

        var url = Environment.GetEnvironmentVariable("SEQ_URL");
        var apiKey = Environment.GetEnvironmentVariable("SEQ_API_KEY");
        if (url is not null && apiKey is not null)
        {
            conf = conf.WriteTo.Seq(url, LogEventLevel.Information, apiKey: apiKey);
        }

        setter(conf);

        AppDataRecord.Parse(AppDomain.CurrentDomain.BaseDirectory).Enrich(conf);

        return services
            .AddSerilog(dispose: true)
            .AddLogging();

        static void AddFileLogging(LoggerConfiguration conf)
        {
            var logPath = Path.Combine(Path.GetTempPath(), "TestLogs", "logs.txt");
            conf.WriteTo.File(
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
        }
    }

    public static IServiceCollection AddTestDatabase(this IServiceCollection services, string? fileName = null)
    {
        var name = fileName ?? Guid.NewGuid().ToString();
        return services.AddDbContext<LocalDbCtx>(x => x
                .UseSqlite($"DataSource={name}")
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
            )
            .AddDbContextFactory<LocalDbCtx>();
    }

    public static async Task GlobalTearDown()
    {
        await Log.CloseAndFlushAsync();
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