namespace BuildTests.Attributes;

public sealed class SkipIfDev : FactAttribute
{
    public SkipIfDev(string reason = "Only runs in non-development environments")
    {
        var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var overrideSkip = Environment.GetEnvironmentVariable("OVERRIDE_SKIP");
        var isDevelopment = dotnetEnvironment?.Equals("Development", StringComparison.OrdinalIgnoreCase) is true;
        if (isDevelopment && overrideSkip is not "1")
        {
            Skip = reason;
        }
    }
}