using BuildLib.AndroidPublish;
using BuildLib.Logging;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.Options;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.SolutionBuild;

public record DotnetPublishOptions
{
    public bool NoLogo { get; init; } = true;
    public required Action<OutputType, string> ProcessLogger { get; init; }
    public required string PublishConfiguration { get; init; }
    public required AndroidSigningKeyStoreOptions AndroidSigningKeyStoreOptions { get; init; }
    public required string ProjectPath { get; init; }
    public required string AndroidFramework { get; init; }

    public DotNetPublishSettings ToDotNetPublishSettings() =>
        new DotNetPublishSettings()
            .SetNoLogo(NoLogo)
            .SetProject(ProjectPath)
            .SetConfiguration(PublishConfiguration)
            .SetProcessLogger(ProcessLogger)
            .SetFramework(AndroidFramework)
            .SetProperties(AndroidSigningKeyStoreOptions.ToPropertyDictionary());
}

[Inject]
public class DotNetPublishOptionsFactory(
    ProcessLogger<DotNetPublishOptionsFactory> processLogger,
    IOptions<AppConfiguration> configs,
    ReleaseProject project,
    AndroidSigningKeyStoreOptions androidSigningKeyStoreOptions)
{
    public DotnetPublishOptions Create() =>
        new()
        {
            AndroidFramework = configs.Value.AndroidFramework,
            ProcessLogger = processLogger.Log,
            ProjectPath = project.Value.Path,
            PublishConfiguration = configs.Value.PublishConfiguration,
            AndroidSigningKeyStoreOptions = androidSigningKeyStoreOptions
        };
}