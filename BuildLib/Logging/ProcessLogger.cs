using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;

namespace BuildLib.Logging;

[Inject]
public class ProcessLogger<T>(ILogger<T> logger)
{
    public void Log(Output output)
    {
        if (output.Type is OutputType.Err)
        {
            logger.LogError("{Text}", output.Text);
            return;
        }

        logger.LogDebug("{Text}", output.Text);
    }

    public void Log(OutputType type, string text)
    {
        var output = new Output
        {
            Type = type,
            Text = text
        };
        Log(output);
    }
}