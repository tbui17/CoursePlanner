using System.Text;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Json;
using Serilog.Sinks.File;

namespace Lib.Config;

public record FileSinkOptions
{
    public ITextFormatter Formatter { get; init; } = new JsonFormatter();
    public string Path { get; init; } = "log.txt";
    public LogEventLevel RestrictedToMinimumLevel { get; init; } = LogEventLevel.Verbose;
    public long? FileSizeLimitBytes { get; init; } = 1073741824L;
    public LoggingLevelSwitch? LevelSwitch { get; init; } = null;
    public bool Buffered { get; init; }
    public bool Shared { get; init; }
    public TimeSpan? FlushToDiskInterval { get; init; } = null;
    public RollingInterval RollingInterval { get; init; } = RollingInterval.Infinite;
    public bool RollOnFileSizeLimit { get; init; }
    public int? RetainedFileCountLimit { get; init; } = 31;
    public Encoding? Encoding { get; init; }
    public FileLifecycleHooks? Hooks { get; init; } = null;
    public TimeSpan? RetainedFileTimeLimit { get; init; }
}