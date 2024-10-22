using BuildLib.Commands;
using BuildLib.Logging;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using CliWrap.Builders;
using Cocona;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Serilog;

ConfigLogging();

var container = Container.Init();

var app = CoconaApp.CreateBuilder().Build();


app.AddCommand("example_command",
    () =>
    {
        var logger = ResolveLogger<CoconaApp>();
        logger.LogInformation("Hello, World!");
    }
);
app.AddCommand("publish",
    async () =>
    {
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecuteAsync();
    }
);

app.AddCommand("upload",
    async () =>
    {
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecuteUploadAsync();
    }
);

app.AddCommand("publish_upload",
    async () =>
    {
        var cmd = Resolve<PublishCommand>();
        await cmd.ExecutePublishAndUploadAsync();
    }
);

app.AddCommand("increment_version",
    async (VersionType versionType) =>
    {
        var project = Resolve<IMsBuildProject>();

        Action act = versionType switch
        {
            VersionType.Major => project.IncrementMajor,
            VersionType.Minor => project.IncrementMinor,
            VersionType.Patch => project.IncrementPatch,
            _ => throw new ArgumentOutOfRangeException(nameof(versionType), versionType, null)
        };

        act();
        await Task.CompletedTask;
    }
);

app.AddCommand("migrate",
    (string migrationName, string project = "Lib", string context = "LocalDbCtx") =>
    {
        var solution = Resolve<Solution>();

        DotNetTasks.DotNet(
            arguments: $"ef migrations add {migrationName} --project {project} --context {context}",
            workingDirectory: solution.Directory,
            logger: Resolve<ProcessLogger<DotNetTasks>>().Log
        );
    }
);


try
{
    app.Run();
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;


T Resolve<T>() where T : notnull => container.Resolve<T>();

ILogger<T> ResolveLogger<T>() => container.Resolve<ILogger<T>>();

void ConfigLogging()
{
    var config = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.Console(LogUtil.DefaultExpressionTemplate);

    var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

    if (env is "Development")
    {
        Console.WriteLine("Enabling debug logging");
        config = config.MinimumLevel.Debug();
    }

    Log.Logger = config.CreateLogger();
}

public static class ArgumentsBuilderExtensions
{
    public static ArgumentsBuilder Add(this ArgumentsBuilder builder, IDictionary<string, string> args)
    {
        var args2 = args
            .SelectKeys(x =>
                {
                    if (x.Key.StartsWith('-'))
                    {
                        return x.Key;
                    }

                    return "--" + x.Key;
                }
            );
        foreach (var (key, value) in args2)
        {
            builder
                .Add(key)
                .Add(value);
        }

        return builder;
    }
}