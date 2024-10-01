using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Serilog;
using Utils;

[GitHubActions(
    "publish",
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = true,
    On = [GitHubActionsTrigger.Push, GitHubActionsTrigger.WorkflowDispatch],
    InvokedTargets = [nameof(Publish)],
    ImportSecrets =
    [
        nameof(EnvVars.COURSEPLANNER_KEYSTORE_CONTENTS_BASE64),
        nameof(EnvVars.COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_BASE64),
        nameof(EnvVars.COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS),
        nameof(EnvVars.COURSEPLANNER_KEY),
        nameof(EnvVars.COURSEPLANNER_APPLICATION_ID)
    ],
    OnPushBranches = [RepoBranches.PublishCi],
    EnableGitHubToken = true,
    ConcurrencyGroup = "publish",
    ConcurrencyCancelInProgress = true
)]
class Build : NukeBuild
{
    [Parameter("Target framework for Android build")] readonly string AndroidFramework = "net8.0-android";

    [Parameter("Android signing key store path")] readonly string AndroidSigningKeyStore = "courseplanner.keystore";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] [Secret] readonly string COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS;
    [Parameter] [Secret] readonly string COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_BASE64;
    [Parameter] [Secret] readonly string COURSEPLANNER_KEY;

    [Parameter] [Secret] readonly string COURSEPLANNER_KEYSTORE_CONTENTS_BASE64;

    [PathVariable] readonly Tool Gh;

    [Solution(GenerateProjects = true)] readonly Solution Solution;

    [Parameter("User identifier for OAuth client")] readonly string UserIdentifier = "service_account";

    [CanBeNull] private AndroidDirectoryManager _androidDirectory;

    GitHubActions GitHubActions => GitHubActions.Instance;

    AbsolutePath OutputDirectory => Solution.Directory / "output";
    AndroidDirectoryManager AndroidDirectory => _androidDirectory ??= new(() => OutputDirectory);


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
                    { EnvVars.COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_BASE64.ToString(), base64ServiceAccountKey },
                    { EnvVars.COURSEPLANNER_KEYSTORE_CONTENTS_BASE64.ToString(), base64Store },
                    { EnvVars.COURSEPLANNER_APPLICATION_ID.ToString(), EnvVars.COURSEPLANNER_APPLICATION_ID.Get() }
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

    Target InstallMauiWorkload => _ => _
        .DependsOn(EnsureOAuthClient)
        .Executes(() => DotNetTasks.DotNetWorkloadInstall(x => x.AddWorkloadId("maui")));

    Target BuildAndroidPackage => _ => _
        .DependsOn(InstallMauiWorkload)
        .Produces([
                AndroidDirectory.PackagePattern,
                AndroidDirectory.BundlePattern
            ]
        )
        .Executes(() =>
            {
                using var _ = Solution.Directory.SwitchWorkingDirectory();

                DotNetTasks.DotNetBuild(x => x
                    .SetProjectFile(Solution.CoursePlanner)
                    .SetConfiguration(Configuration)
                    .SetFramework(AndroidFramework)
                    .SetProperties(CreateAndroidBuildProperties().ToPropertyDictionary())
                    .SetOutputDirectory(AndroidDirectory.OutputDirectory)
                );
            }
        );

    Target EnsureOAuthClient => _ => _
        .Executes(async () =>
            {
                var clientSecrets = GoogleClientSecrets.FromStream(
                    new MemoryStream(Convert.FromBase64String(COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_BASE64))
                );
                var cred = await GoogleWebAuthorizationBroker.AuthorizeAsync(clientSecrets.Secrets,
                    [AndroidPublisherService.Scope.Androidpublisher],
                    UserIdentifier,
                    CancellationToken.None
                );

                var service = new AndroidPublisherService(new BaseClientService.Initializer
                    {
                        HttpClientInitializer = cred,
                        ApplicationName = nameof(Build),
                    }
                );
                service.Edits.Insert(new AppEdit() { Id = Guid.NewGuid().ToString(), ExpiryTimeSeconds = "60" },
                    EnvVars.COURSEPLANNER_APPLICATION_ID.ToString()
                );
            }
        );


    Target Publish => _ => _
        .Consumes(BuildAndroidPackage)
        .DependsOn(EnsureOAuthClient)
        .Executes(() =>
            {
                var files = AndroidDirectory.GetOrThrowAndroidFiles();
                Log.Information("Found {Count} Android files: {Files}", files.Count, files);
            }
        );

    public static int Main() => Execute<Build>();

    private DotnetAndroidBuildProperties CreateAndroidBuildProperties()
    {
        var properties = new DotnetAndroidBuildProperties
        {
            AndroidSigningKeyStore = AndroidSigningKeyStore,
            AndroidSigningKeyAlias = COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS,
            AndroidSigningKeyPass = COURSEPLANNER_KEY,
            AndroidSigningStorePass = COURSEPLANNER_KEY
        };
        return properties;
    }
}

public class AndroidDirectoryManager(Func<AbsolutePath> outputDirectory)
{
    public AbsolutePath OutputDirectory => outputDirectory();
    public AbsolutePath PackagePattern => OutputDirectory / "*.apk";
    public AbsolutePath BundlePattern => OutputDirectory / "*.apb";

    public IReadOnlyCollection<AbsolutePath> GetBundles()
    {
        return BundlePattern.GlobFiles();
    }

    public IReadOnlyCollection<AbsolutePath> GetPackages()
    {
        return PackagePattern.GlobFiles();
    }

    public IReadOnlyCollection<AbsolutePath> GetAndroidFiles()
    {
        return GetBundles().Concat(GetPackages()).ToList();
    }

    public IReadOnlyCollection<AbsolutePath> GetOrThrowAndroidFiles()
    {
        var res = GetBundles().Concat(GetPackages()).ToList();

        if (res.Count == 0)
        {
            throw new Exception("No Android files found.");
        }

        return res;
    }
}

public record DotnetAndroidBuildProperties
{
    public required string AndroidSigningKeyStore { get; init; }
    public required string AndroidSigningKeyAlias { get; init; }
    public required string AndroidSigningKeyPass { get; init; }
    public required string AndroidSigningStorePass { get; init; }
}

enum EnvVars
{
    COURSEPLANNER_ANDROID_SIGNING_KEY_STORE,
    COURSEPLANNER_KEY,
    COURSEPLANNER_ANDROID_SIGNING_KEY_ALIAS,
    COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_JSON,
    COURSEPLANNER_GOOGLE_SERVICE_ACCOUNT_BASE64,
    COURSEPLANNER_KEYSTORE_CONTENTS_BASE64,
    COURSEPLANNER_APPLICATION_ID
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