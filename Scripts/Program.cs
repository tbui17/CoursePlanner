using BuildLib.Commands;
using BuildLib.Logging;
using BuildLib.SolutionBuild;
using BuildLib.Utils;
using Cocona;
using Microsoft.Extensions.Logging;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Docker;
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

class P2
{
    private static void Build()
    {
        var buildArgs = new Dictionary<string, string>
        {
            { "KEY_URI", Environment.GetEnvironmentVariable("KeyUri") }
        };

        var secrets = new Dictionary<string, string>
        {
            { "TENANT_ID", Environment.GetEnvironmentVariable("TENANT_ID") },
            { "SERVICE_PRINCIPAL_ID", Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_ID") },
            { "SERVICE_PRINCIPAL_SECRET", Environment.GetEnvironmentVariable("SERVICE_PRINCIPAL_SECRET") }
        };

        string buildStr = string.Join(" ", buildArgs.Select(kv => $"--build-arg '{kv.Key}={kv.Value}'"));
        string secretStr = string.Join(" ", secrets.Keys.Select(k => $"--secret id={k},env={k}"));

        var additionalArgs = $"{buildStr} {secretStr}";
        var args = new List<string>
        {
            "docker build .",
            $"--file Scripts/Dockerfile",
            "--tag build_image",
            "--progress=plain",
            additionalArgs
        };


        new DockerBuildSettings()
            .SetFile("Scripts/Dockerfile")
            .SetTag("build_image")
            .SetProgress(ProgressType.plain)
            .SetBuildArg(buildArgs.Select(x => $"{x.Key}={x.Value}").ToArray())
            .SetSecret(secretStr);

        var expr = string.Join(" ", args);
        Console.WriteLine($"Executing {expr}");
    }

    private static void Auth()
    {
        var keys = new List<string> { "TENANT_ID", "SERVICE_PRINCIPAL_ID", "SERVICE_PRINCIPAL_SECRET" };
        var vars = new Dictionary<string, string>();

        foreach (var key in keys)
        {
            vars[key] = File.ReadAllText($"/run/secrets/{key}");
        }

        var command =
            $"az login --service-principal --username {vars["SERVICE_PRINCIPAL_ID"]} --password {vars["SERVICE_PRINCIPAL_SECRET"]} --tenant {vars["TENANT_ID"]}";
    }

    private static void StartApp()
    {
        var command = $"dotnet run --verbosity normal --project Scripts -- example_command ";
    }
}