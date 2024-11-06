using BuildLib.Commands;
using BuildLib.Logging;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Cocona;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Scripts.Commands;
using Serilog;

namespace Scripts;

public class Application
{
    private Container? _container;

    public Application()
    {
        App.AddCommand("example_command",
            () =>
            {
                var logger = ResolveLogger<CoconaApp>();
                logger.LogInformation("Hello, World!");
            }
        );
        App.AddCommand("publish",
            async () =>
            {
                var cmd = Resolve<PublishCommand>();
                await cmd.ExecuteAsync();
            }
        );

        App.AddCommand("upload",
            async () =>
            {
                var cmd = Resolve<PublishCommand>();
                await cmd.ExecuteUploadAsync();
            }
        );

        App.AddCommand("publish_upload",
            async () =>
            {
                var cmd = Resolve<PublishCommand>();
                await cmd.ExecutePublishAndUploadAsync();
            }
        );

        App.AddCommand("increment_version",
            async ([Argument] VersionType versionType) =>
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

        App.AddCommand("migrate",
            (
                [Argument] string migrationName,
                [Argument] string project = "Lib",
                [Argument] string context = "LocalDbCtx"
            ) =>
            {
                var solution = Resolve<Solution>();

                DotNetTasks.DotNet(
                    arguments: $"ef migrations add {migrationName} --project {project} --context {context}",
                    workingDirectory: solution.Directory,
                    logger: Resolve<ProcessLogger<DotNetTasks>>().Log
                );
            }
        );

        App.AddCommand("create_action",
            async ([Argument] string actionName) =>
            {
                var cmd = Resolve<CreateActionCommand>();
                await cmd.ExecuteAsync(actionName);
            }
        );

        ConfigLogging();
    }

    private Container Container => _container ??= Container.Init();
    private CoconaApp App { get; } = CoconaApp.CreateBuilder().Build();

    public T Resolve<T>() where T : notnull => Container.Resolve<T>();
    public ILogger<T> ResolveLogger<T>() => Container.Resolve<ILogger<T>>();

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

    public async Task ExecuteAsync()
    {
        try
        {
            await App.RunAsync();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}