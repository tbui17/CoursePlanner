namespace BuildTests.Attributes;

public sealed class Integration : FactAttribute
{
    public Integration(string reason = "Only runs in non-development environments")
    {
        var dotnetEnvironment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var runIntegrationTestsOverride = Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS_OVERRIDE");
        var isDevelopment = dotnetEnvironment?.Equals("Development", StringComparison.OrdinalIgnoreCase) is true;
        if (isDevelopment && runIntegrationTestsOverride is not "1")
        {
            SkipTest();
        }

        return;

        void SkipTest()
        {
            Skip = reason;
        }
    }
}