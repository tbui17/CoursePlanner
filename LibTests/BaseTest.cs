using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;


namespace LibTests;

public abstract class BaseTest : BaseConfigTest, IBaseTest
{
    protected override IDisposable TestContextProvider()
    {
        var test = TestContext.CurrentContext.Test;

        return CreateLogContext(new()
        {
            ["TestId"] = test.ID,
            ["TestName"] = test.Name,
            ["TestClass"] = test.ClassName,
            ["TestMethodName"] = test.MethodName,
            ["TestArguments"] = test.Arguments,
            ["TestFullName"] = test.FullName,
            ["TestProperties"] = test.Properties,

        });
    }

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
    }

    [TearDown]
    public override async Task TearDown()
    {

        await base.TearDown();
    }
}