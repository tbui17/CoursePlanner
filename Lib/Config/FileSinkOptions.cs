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
    /// <summary>
    /// A formatter, such as <see cref="JsonFormatter"/>, to convert the log events into
    /// text for the file.
    /// </summary>
    public ITextFormatter Formatter { get; init; } = new JsonFormatter();

    /// <summary>
    /// Path to the file.
    /// </summary>
    public string Path { get; init; } = "log.txt";

    /// <summary>
    /// The minimum level for events passed through the sink.
    /// </summary>
    public LogEventLevel RestrictedToMinimumLevel { get; init; } = LogEventLevel.Verbose;

    /// <summary>
    /// The approximate maximum size, in bytes, to which a log file will be allowed to grow.
    /// For unrestricted growth, pass null. The default is 1 GB. To avoid writing partial events, the last event within the limit
    /// will be written in full even if it exceeds the limit.
    /// </summary>
    public long? FileSizeLimitBytes { get; init; } = 1073741824L;

    /// <summary>
    /// A switch allowing the pass-through minimum level to be changed at runtime.
    /// </summary>
    public LoggingLevelSwitch? LevelSwitch { get; init; } = null;

    /// <summary>
    /// Indicates if flushing to the output file can be buffered or not. The default is false.
    /// </summary>
    public bool Buffered { get; init; }

    /// <summary>
    /// Allow the log file to be shared by multiple processes. The default is false.
    /// </summary>
    public bool Shared { get; init; }

    /// <summary>
    /// If provided, a full disk flush will be performed periodically at the specified interval.
    /// </summary>
    public TimeSpan? FlushToDiskInterval { get; init; } = null;

    /// <summary>
    /// The interval at which logging will roll over to a new file.
    /// </summary>
    public RollingInterval RollingInterval { get; init; } = RollingInterval.Infinite;

    /// <summary>
    /// If <code>true</code>, a new file will be created when the file size limit is reached.
    /// Filenames will have a number appended in the format <code>_NNN</code>, with the first filename given no number.
    /// </summary>
    public bool RollOnFileSizeLimit { get; init; }

    /// <summary>
    /// The maximum number of log files that will be retained, including the current log file. For unlimited retention, pass null.
    /// The default is 31.
    /// </summary>
    public int? RetainedFileCountLimit { get; init; } = 31;

    /// <summary>
    /// Character encoding used to write the text file. The default is UTF-8 without BOM.
    /// </summary>
    public Encoding? Encoding { get; init; }

    /// <summary>
    /// Optionally enables hooking into log file lifecycle events.
    /// </summary>
    public FileLifecycleHooks? Hooks { get; init; } = null;

    /// <summary>
    /// The maximum time after the end of an interval that a rolling log file will be retained. Must be greater than or equal to <see cref="TimeSpan.Zero"/>.
    /// Ignored if <paramref see="rollingInterval"/> is <see cref="RollingInterval.Infinite"/>. The default is to retain files indefinitely.
    /// </summary>
    public TimeSpan? RetainedFileTimeLimit { get; init; }
}