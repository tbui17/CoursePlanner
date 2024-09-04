using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;


namespace BaseTestSetup;

using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

public static class BaseTestConfig
{
    public static IServiceCollection AddLogger(this IServiceCollection services, Action<Logger> setter)
    {
        var conf = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithEnvironmentName()
            .Enrich.WithProperty("FriendlyApplicationName", AppDomain.CurrentDomain.FriendlyName)
            .Enrich.With(new StackTraceEnricher())
            .WriteTo.Console(LogEventLevel.Information)
            .WriteTo.Debug(LogEventLevel.Debug);

        AppDataRecord.Parse(AppDomain.CurrentDomain.BaseDirectory).Enrich(conf);

        if (Environment.GetEnvironmentVariable("SEQ_URL") is { } url)
        {
            conf.WriteTo.Seq(url,LogEventLevel.Information);
        }

        setter(conf.CreateLogger());

        return services.AddSerilog().AddLogging();
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

    public void Enrich(LoggerConfiguration conf)
    {
        foreach (var (name, val) in GetType().GetProperties().Select(x => (x.Name, x.GetValue(this))))
        {
            conf.Enrich.WithProperty(name, val);
        }
    }
}