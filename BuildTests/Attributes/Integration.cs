using BuildLib.Utils;

namespace BuildTests.Attributes;

public sealed class Integration : FactAttribute
{
    public Integration()
    {
        var runIntegrationTestsOverride = Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS_OVERRIDE") ?? "";
        if (runIntegrationTestsOverride.EqualsIgnoreCase("true"))
        {
            return;
        }

        SkipTest();
        return;

        void SkipTest()
        {
            Skip =
                "This test is to be run locally. To enable automatic execution, set the RUN_INTEGRATION_TESTS_OVERRIDE environment variable to 'true'.";
        }
    }
}