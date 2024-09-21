using BaseTestSetup;
using Lib.Attributes;
using Lib.Config;
using Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;


namespace LibTests;

public abstract class BaseTest : BaseConfigTest, IBaseTest
{

    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
    }

    [TearDown]
    public override async Task TearDown()
    {
        await base.Setup();
    }
}