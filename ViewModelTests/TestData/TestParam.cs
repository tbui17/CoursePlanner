namespace ViewModelTests.TestData;

public class TestParam
{
    public static IEnumerable<TestCaseData> NameAndDate()
    {
        return new[]
        {
            new TestCaseData(
                "",
                DateTime.Now,
                DateTime.Now.AddMinutes(1)
            ).SetName("EmptyName"),
            new TestCaseData(
                "   ",
                DateTime.Now,
                DateTime.Now.AddMinutes(1)
            ).SetName("Whitespace"),
            new TestCaseData(
                "Valid Name",
                DateTime.Now.AddMinutes(1),
                DateTime.Now
            ).SetName("StartAfterEnd"),
            new TestCaseData(
                "",
                DateTime.Now.AddMinutes(1),
                DateTime.Now
            ).SetName("AllInvalid"),
        };
    }
}