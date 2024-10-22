using BuildLib.Utils;

namespace BuildTests.Attributes;

public sealed class ManualTest : FactAttribute
{
    public ManualTest(bool enable = false)
    {
        if (Environment.GetEnvironmentVariable("RUN_MANUAL_TESTS_OVERRIDE").IsTrueString())
        {
            return;
        }

        if (enable)
        {
            return;
        }

        Skip =
            "This test is for manual testing only. To run this test, set the 'RUN_MANUAL_TESTS_OVERRIDE' environment variable to 'true' or pass true to the attribute's constructor.";
    }
}