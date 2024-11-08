using BuildLib.AndroidPublish;
using BuildLib.Logging;
using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.Options;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;

namespace BuildLib.SolutionBuild;

[Inject]
public class DotNetPublishSettingsFactory(
    ProcessLogger<DotNetPublishSettingsFactory> processLogger,
    IOptions<DotnetPublishAndroidConfiguration> configs,
    ReleaseProject project,
    AndroidSigningKeyStoreOptions androidSigningKeyStoreOptions
)
{
    public DotNetPublishSettings Create()
    {
        return new DotNetPublishSettings()
            .EnableNoLogo()
            .SetProject(project.Value.Path)
            .SetConfiguration(configs.Value.Configuration)
            .SetProcessLogger(processLogger.Log)
            .SetFramework(configs.Value.AndroidFramework)
            .SetProperties(androidSigningKeyStoreOptions.ToPropertyDictionary())
            .SetProcessWorkingDirectory(project.Value.Solution.Directory)
            .SetProcessExitHandler(ExitHandler);
    }

    private void ExitHandler(IProcess p)
    {
        // ! can potentially log secrets
        if (p.ExitCode is not 0)
        {
            var msg = new
            {
                p.ExitCode,
                p.Output,
                p.Arguments,
                p.Id,
                p.FileName,
                p.HasExited,
                p.WorkingDirectory
            };
            var jsonMsg = msg.ToJson();
            processLogger.Log(OutputType.Err, "Dotnet publish failed");
            processLogger.Log(OutputType.Err, jsonMsg);
            try
            {
                p.AssertZeroExitCode();
            }
            catch (ProcessException e)
            {
                throw new ApplicationException(jsonMsg, e);
            }
        }

        processLogger.Log(OutputType.Std, "Dotnet publish succeeded");
    }
}