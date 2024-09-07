using BaseTestSetup;

[assembly: Parallelizable(ParallelScope.Fixtures)]

namespace MauiConfigTests;

[SetUpFixture]
public class GlobalSetup
{
    [OneTimeTearDown]
    public async Task GlobalTearDown()
    {
        await BaseTestConfig.GlobalTearDown();
    }
}