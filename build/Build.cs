using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using Utils;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;

[GitHubActions(
    "publish",
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    On = [GitHubActionsTrigger.Push,GitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(Publish)],
    ImportSecrets = [nameof(EnvVars.COURSEPLANNER_KEYSTORE_CONTENTS_BASE64),nameof(EnvVars.COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_JSON)],
    OnPushBranches = [RepoBranches.PublishCi],
    EnableGitHubToken = true

)]
class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.UploadSecrets);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [PathVariable] readonly Tool Gh;

    [Solution] readonly Solution Solution;


    Target UpdateSecrets => _ => _
        .Executes(() =>
            {
                var dir = Solution.Directory!;


                AbsolutePath keyStorePath = EnvVars.COURSEPLANNER_ANDROID_SIGNING_KEY_STORE.Get();
                var coursePlannerKey = EnvVars.COURSEPLANNER_KEY.Get();
                var coursePlannerAndroidSigningKeyAlias = EnvVars.COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS.Get();
                AbsolutePath coursePlannerGoogleServiceAccountJsonPath =
                    EnvVars.COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_JSON.Get();


                using var _ = dir.SwitchWorkingDirectory();
                var base64Store = keyStorePath.ReadAllBytes().Thru(Convert.ToBase64String);
                // var ascFilePath = keyStorePath / ".asc";
                var base64ServiceAccountKey =
                    coursePlannerGoogleServiceAccountJsonPath.ReadAllBytes().Thru(Convert.ToBase64String);

                var secrets = new Dictionary<string, string>
                {
                    { EnvVars.COURSEPLANNER_ANDROID_SIGNING_KEY_STORE.ToString(), keyStorePath },
                    { EnvVars.COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS.ToString(), coursePlannerAndroidSigningKeyAlias },
                    { EnvVars.COURSEPLANNER_KEY.ToString(), coursePlannerKey },
                    { EnvVars.COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_JSON.ToString(), base64ServiceAccountKey },
                    { EnvVars.COURSEPLANNER_KEYSTORE_CONTENTS_BASE64.ToString(), base64Store }
                };
                var content = secrets.Select(x => $"{x.Key}={x.Value}").ToList();

                var secretsFilePath = dir / "gh.secrets";
                var envFilePath = dir / ".env";


                secretsFilePath.WriteAllLines(content);
                envFilePath.WriteAllLines(content);

                Log.Information("Secrets written to paths: {SecretsFilePath}, {EnvFilePath}", secretsFilePath,
                    envFilePath
                );
            }
        );

    Target UploadSecrets => _ => _
        .Executes(() =>
            {
                using var _ = Solution.Directory.SwitchWorkingDirectory();
                var envFilePath = Solution.Directory / ".env";
                if (!File.Exists(envFilePath))
                {
                    throw new ArgumentException($"File {envFilePath} does not exist.");
                }

                Log.Information("Found .env file at {EnvFilePath}", envFilePath);
                Log.Information("Uploading secrets to GitHub through gh CLI...");
                Gh("secret set -f .env ");
                Log.Information("Secrets uploaded to GitHub.");
            }
        );

    Target Publish => _ => _
        .Executes(() =>
            {

            }
        );
}

enum EnvVars
{
    COURSEPLANNER_ANDROID_SIGNING_KEY_STORE,
    COURSEPLANNER_KEY,
    COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS,
    COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_JSON,
    COURSEPLANNER_KEYSTORE_CONTENTS_BASE64
}

static class EnvVarExtensions
{
    public static string Get(this EnvVars envVar)
    {
        var s = Environment.GetEnvironmentVariable(envVar.ToString())?.Trim();

        if (string.IsNullOrWhiteSpace(s))
        {
            throw new Exception($"Environment variable {envVar} is missing.");
        }

        return s;
    }
}

public static class RepoBranches
{
    public const string Main = "main";
    public const string Current = "current";
    public const string PublishCi = "publish-ci";
}