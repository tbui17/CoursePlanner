using BuildLib.Secrets;
using BuildLib.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nuke.Common.ProjectModel;

namespace BuildLib.SolutionBuild;

[Inject(Lifetime = ServiceLifetime.Singleton)]
public class KeyFileContextFactory(
    ILogger<KeyFileContextFactory> logger,
    IOptions<DotnetPublishAndroidConfiguration> config,
    ILoggerFactory loggerFactory,
    Solution solution)
{
    public KeyFileContext Create()
    {
        var path = config.Value.AndroidSigningKeyStore.Thru(string (x) => Path.IsPathFullyQualified(x)
            ? x
            : solution.Directory / x
        );

        var keyFileContext = new KeyFileContext
        {
            KeyFile = new FileInfo(path),
            Base64Text = config.Value.KeystoreFile,
            Logger = loggerFactory.CreateLogger<KeyFileContext>()
        };
        logger.LogDebug("Created key file context with path {Path} and base64 text of length {Length}",
            keyFileContext.KeyFile,
            keyFileContext.Base64Text.Length
        );
        return keyFileContext;
    }
}