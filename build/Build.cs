using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BuildLib.Secrets;
using BuildLib.Serialization;
using BuildLib.Utils;
using CaseConverter;
using Entry.FileSystem;
using Google.Apis.AndroidPublisher.v3;
using Google.Apis.AndroidPublisher.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using JetBrains.Annotations;
using MoreLinq.Extensions;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Pwsh;
using Nuke.Common.Utilities;
using Serilog;

[assembly: InternalsVisibleTo("BuildTests")]

public class Build : NukeBuild
{
    [Parameter] readonly string AndroidFramework;
    [Parameter] readonly string AndroidSigningKeyAlias;

    [Parameter] readonly string AndroidSigningKeyStore;
    [Parameter] readonly string ApplicationId;

    [Parameter] readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [PathVariable] readonly Tool Gh;
    [Parameter] [Secret] readonly string GoogleServiceAccountBase64;
    [Parameter] [Secret] readonly string Key;
    [Parameter] [Secret] readonly string KeystoreContents;

    [Solution("CoursePlanner.sln")] readonly Solution Solution;


    [Parameter] readonly string UserIdentifier;
    [CanBeNull] private Container _container;


    GitHubActions GitHubActions => GitHubActions.Instance;


    Container Container => _container ??= Container.Init<Build>();

    public Target Publish => _ => _
        .DependsOn(BuildAndroidPackage)
        .Executes(() =>
            {
                var androidDirectory = new AndroidDirectoryManager(Solution.Directory);
                var files = androidDirectory.GetOrThrowAndroidFiles();
                Log.Information("Found {FileCount} files: {Files}", files.Count, files.Select(x => x.Name).ToList());
                //
                // var filePath = files
                //     .Where(x => x.ToString().EndsWith(".aab"))
                //     .Single(x => x.Contains("Signed"));
                //
                // Log.Information("Found AAB file at {FilePath}", filePath);
                //
                // filePath.CopyToDirectory(OutputDirectory);
                // Log.Information("Copied AAB file to {OutputDirectory}", OutputDirectory);
            }
        );

    public Target UpdateEnv => _ => _
        .Executes(() =>
            {
                var secrets = Container.GetConfig<CoursePlannerSecrets>();
                var serializer = Container.Resolve<SnakeCaseSerializer>();
                var envFilePath = Solution.Directory / ".env";
                var actEnv = Solution.Directory / "gh.secrets";


                var text = secrets
                    .ToPropertyDictionary()
                    .SelectKeys(x => x.Key.ToSnakeCase().ToUpperInvariant())
                    .SelectValues(x => EscapeJson(x.Value))
                    .Select(x => $"{x.Key}={x.Value}")
                    .ToList();

                actEnv.WriteAllLines(text);
                envFilePath.WriteAllLines(text);


                Log.Information("Updated .env file at {EnvFilePath} {ActEnvFilePath}", envFilePath, actEnv);
                return;

                string EscapeJson(object value)
                {
                    if (value is string s)
                    {
                        return s;
                    }

                    var serialized = serializer.Serialize(value);

                    if (value.GetType().IsPrimitive)
                    {
                        return serialized;
                    }

                    var escaped = serialized.Replace("'", "\\'");
                    return escaped.WrapIn("'");
                }
            }
        );

    public Target B64GoogleServiceAccount => _ => _
        .Executes(() =>
            {
                var dir = Solution.Directory!;
                var secrets = Container.GetConfig<CoursePlannerSecrets>();

                using var _ = dir.SwitchWorkingDirectory();
                var serializer = Container.Resolve<SnakeCaseSerializer>();

                serializer
                    .Serialize(secrets.GoogleServiceAccount)
                    .Thru(Encoding.UTF8.GetBytes)
                    .Thru(Convert.ToBase64String)
                    .Tap(x => SetJsonSecrets(new()
                            {
                                [secrets.ConfigurationKeyName(x => x.GoogleServiceAccountBase64)] = x
                            }
                        )
                    );
            }
        );

    public Target UploadToAzure => _ => _
        .Executes(async () =>
            {
                var dir = Solution.Directory!;
                using var _ = dir.SwitchWorkingDirectory();
                var secrets = Container.GetConfig<CoursePlannerSecrets>();


                var client = new SecretClient(new Uri(secrets.KeyUri), new DefaultAzureCredential());


                var resp = await secrets
                    .ToPropertyDictionary()
                    .SelectValues(x => x.Value as string ?? x.Value.ToJson())
                    .SelectValues(x => client.SetSecretAsync(x.Key, x.Value))
                    .Thru(Task.WhenAll);

                var (s, f) = resp
                    .Partition(x => x.Value.HasValue);

                var failed = f.ToList();
                var success = s.ToList();


                Log.Information("Successfully uploaded {SuccessCount} / {TotalCount} secrets: {SecretNames}",
                    success.Count,
                    success.Count + failed.Count,
                    success.Select(x => x.Key).ToList()
                );

                if (failed.Count > 0)
                {
                    Log.Error("Failed to upload {FailedCount} secrets: {SecretNames}",
                        failed.Count,
                        failed.Select(x => x.Key).ToList()
                    );
                    foreach (var (key, value) in failed)
                    {
                        Log.Error("Failed to upload secret {Key}: {Value}", key,
                            value.GetRawResponse().Content.ToString()
                        );
                    }

                    throw new ApplicationException("Failed to upload secrets to Azure Key Vault.");
                }
            }
        );

    public Target UploadSecrets => _ => _
        .Executes(() =>
            {
                var envFilePath = Solution.Directory / ".env";
                if (!File.Exists(envFilePath))
                {
                    throw new ArgumentException($"File {envFilePath} does not exist.");
                }

                Log.Information("Found .env file at {EnvFilePath}", envFilePath);
                Log.Information("Uploading secrets to GitHub through gh CLI...");
                Gh("secret set -f .env", workingDirectory: Solution.Directory);
                Log.Information("Secrets uploaded to GitHub.");
            }
        );

    public Target InstallMauiWorkload => _ => _
        .OnlyWhenDynamic(() => !IsLocalBuild)
        .Executes(() => DotNetTasks.DotNetWorkloadInstall(x => x.AddWorkloadId("maui")));

    public Target BuildAndroidPackage => _ => _
        .DependsOn(InstallMauiWorkload)
        .Executes(() =>
            {
                var androidDirectory = new AndroidDirectoryManager(Solution.Directory);
                using var _ = Solution.Directory.SwitchWorkingDirectory();

                DotNetTasks.DotNetBuild(x => x
                    .SetProjectFile(Solution.CoursePlanner)
                    .SetConfiguration(Configuration)
                    .SetFramework(AndroidFramework)
                    .SetProperties(CreateAndroidBuildProperties().ToPropertyDictionary())
                    .SetOutputDirectory(androidDirectory.OutputDirectory)
                );
            }
        );

    public Target EnsureOAuthClient => _ => _
        .Executes(async () =>
            {
                var clientSecrets = GoogleClientSecrets.FromStream(
                    new MemoryStream(Convert.FromBase64String(GoogleServiceAccountBase64))
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
                service.Edits.Insert(new AppEdit { Id = Guid.NewGuid().ToString(), ExpiryTimeSeconds = "60" },
                    Container.GetConfig<CoursePlannerSecrets>().ApplicationId
                );
            }
        );


    public static int Main() => Execute<Build>();

    public static int ExecuteTarget(Expression<Func<Build, Target>> target) => Execute(target);

    private IReadOnlyCollection<Output> Exec(string arg) => PwshTasks.Pwsh(x => x
        .SetProcessWorkingDirectory(Solution._build.Directory)
        .SetCommand(arg)
    );

    private static void WithTempFile(Action<AbsolutePath> action)
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            action(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private void SetJsonSecrets(Dictionary<string, object> secrets)
    {
        WithTempFile(path =>
            {
                Log.Information("Writing secrets to {Path}", path);
                path.WriteJson(secrets);
                Log.Information(
                    "Setting secrets from {Path} into secrets store for project {Project} with Id {UserSecretsId}",
                    path, Solution._build.Name, Solution._build.GetProperty("UserSecretsId")
                );
                Exec($"cat {path} | dotnet user-secrets set");
            }
        );
    }


    private DotnetAndroidBuildProperties CreateAndroidBuildProperties()
    {
        var properties = new DotnetAndroidBuildProperties
        {
            AndroidSigningKeyStore = AndroidSigningKeyStore,
            AndroidSigningKeyAlias = AndroidSigningKeyAlias,
            AndroidSigningKeyPass = Key,
            AndroidSigningStorePass = Key
        };
        return properties;
    }
}