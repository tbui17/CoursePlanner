using System.Diagnostics.Tracing;
using Azure.Core.Diagnostics;
using BuildLib.Commands;
using BuildLib.Logging;
using BuildLib.Serialization;
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
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);
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

        App.AddCommand("populate_key_uri",
            async () => { await new PopulateKeyUriCommand().ExecuteAsync(); }
        );

        App.AddCommand("encode_file",
            // ReSharper disable once AsyncVoidLambda
            async (string inputPath, string? outputPath = null) =>
            {
                var log = Log.ForContext<Application>();
                var file = new FileInfo(inputPath);
                var outputFile = outputPath is not null
                    ? new FileInfo(outputPath)
                    : file
                        .FullName
                        .Thru(Path.GetFileNameWithoutExtension)
                        .Thru(x => $"output/{x}.txt")
                        .Thru(x => new FileInfo(x));

                Log.Information("Encoding file {InputPath} and writing to {OutputPath}",
                    file.FullName,
                    outputFile.FullName
                );
                var b64 = await File
                    .ReadAllBytesAsync(file.FullName)
                    .ContinueWith(x => Convert.ToBase64String(x.Result));
                log.Information("Produced base64 string with length {Length}", b64.Length);
                await File.WriteAllTextAsync(outputFile.FullName, b64);
            }
        );

        App.AddCommand("decode",
            async (string base64, string outputPath, bool useOutputFolder) =>
            {
                await EncoderFactory.Create().CreateBase64Decoder(base64, outputPath, useOutputFolder).WriteAsync();
            }
        );

        App.AddCommand("CreateKeystoreFile",
            async () =>
            {
                var fac = Resolve<KeyFileContextFactory>();
                var ctx = fac.Create();
                await ctx.WriteAsync();
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
        var log = Log.ForContext<Application>();
        try
        {
            using var listener =
                new AzureEventSourceListener((_, message) => log.Debug("Azure event: {Message}", message),
                    EventLevel.LogAlways
                );
            await App.RunAsync();
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}