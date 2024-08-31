using System.Collections;
using FluentAssertions;
using Lib.Services;
using Lib.Services.ReportService;

namespace LibTests;

[TestFixture]
public class ReportTest
{

    [Test]
    public async Task METHOD()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetDurationReport();
        res.Should().BeNull();
    }
}