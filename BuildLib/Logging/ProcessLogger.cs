using System.Text.RegularExpressions;
using BuildLib.Utils;
using Microsoft.Extensions.Logging;
using Nuke.Common.Tooling;

namespace BuildLib.Logging;

[Inject]
public partial class ProcessLogger<T>(ILogger<T> logger)
{
    public void Log(Output output)
    {
        if (output.Type is OutputType.Err)
        {
            logger.LogError("{Text}", output.Text);
            return;
        }

        if (IsError(output.Text))
        {
            logger.LogError("{Text}", output.Text);
            return;
        }

        logger.LogInformation("{Text}", output.Text);
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

    private static bool IsError(string text)
    {
        return ErrorCodeRegex().IsMatch(text) && text.ContainsIgnoreCase("error");
    }

    [GeneratedRegex("XA\\d{4}")]
    private static partial Regex ErrorCodeRegex();
}