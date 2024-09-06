using Serilog;
using Serilog.Configuration;

namespace BaseTestSetup;

public static class LoggerSinkConfigurationExtensions
{
    public static LoggerConfiguration File(
        this LoggerSinkConfiguration loggerSinkConfiguration,
        FileSinkOptions options
    ) =>
        loggerSinkConfiguration.File(
            options.Formatter,
            options.Path,
            options.RestrictedToMinimumLevel,
            options.FileSizeLimitBytes,
            options.LevelSwitch,
            options.Buffered,
            options.Shared,
            options.FlushToDiskInterval,
            options.RollingInterval,
            options.RollOnFileSizeLimit,
            options.RetainedFileCountLimit,
            options.Encoding,
            options.Hooks,
            options.RetainedFileTimeLimit
        );
}