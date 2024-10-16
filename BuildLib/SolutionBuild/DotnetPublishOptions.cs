using BuildLib.AndroidPublish;
using BuildLib.Logging;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.Options;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

namespace BuildLib.SolutionBuild;

[Inject]
public class DotNetPublishSettingsFactory(
    ProcessLogger<DotNetPublishSettingsFactory> processLogger,
    IOptions<AppConfiguration> configs,
    ReleaseProject project,
    AndroidSigningKeyStoreOptions androidSigningKeyStoreOptions
)
{
    public DotNetPublishSettings Create() =>
        new DotNetPublishSettings()
            .EnableNoLogo()
            .SetProject(project.Value.Path)
            .SetConfiguration(configs.Value.PublishConfiguration)
            .SetProcessLogger(processLogger.Log)
            .SetFramework(configs.Value.AndroidFramework)
            .SetProperties(androidSigningKeyStoreOptions.ToPropertyDictionary());
}