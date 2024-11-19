using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace BuildLib.SolutionBuild;

public sealed class KeyFileContext : IDisposable
{
    public required FileInfo KeyFile { get; init; }

    public required string Base64Text { get; init; }
    public ILogger<KeyFileContext> Logger { get; init; } = NullLogger<KeyFileContext>.Instance;

    public void Dispose()
    {
        Logger.LogDebug("Deleting key file at {Path}", KeyFile);
        KeyFile.Delete();
    }

    public byte[] GetBytes()
    {
        return Convert.FromBase64String(Base64Text);
    }

    public async Task WriteAsync()
    {
        var contents = GetBytes();
        Logger.LogDebug("Writing contents to {Path} with {Length} bytes", KeyFile.FullName, contents.Length);

        await File.WriteAllBytesAsync(KeyFile.FullName, contents);
    }
}