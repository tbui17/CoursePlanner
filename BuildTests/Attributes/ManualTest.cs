namespace BuildTests.Attributes;

public sealed class ManualTest : FactAttribute
{
    public ManualTest(string reason = "This test is for manual testing only")
    {
        Skip = reason;
    }
}