namespace BuildTests.Attributes;

public sealed class SkipIfDev : FactAttribute
{
    public SkipIfDev(string reason = "Only runs in non-development environments")
    {
        if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") is { } s &&
            s.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            Skip = reason;
        }
    }
}