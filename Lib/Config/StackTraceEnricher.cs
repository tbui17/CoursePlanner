using System.Diagnostics;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Lib.Config;

internal class StackTraceEnricher : ILogEventEnricher
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