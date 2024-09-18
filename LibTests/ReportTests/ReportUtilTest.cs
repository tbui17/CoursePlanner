using FluentAssertions;
using Lib.Models;

namespace LibTests.ReportTests;

public class ReportUtilTest
{
    [Test]
    public void MethodsRetrieved()
    {
        new ReportBoundaryUtil(new DurationReport()).GetSubAssertions().Should().NotBeEmpty();
    }
}