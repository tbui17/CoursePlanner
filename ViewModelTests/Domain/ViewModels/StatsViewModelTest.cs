using ViewModels.Domain;
using ViewModelTests.TestSetup;

namespace ViewModelTests.Domain.ViewModels;

public class StatsViewModelTest : BasePageViewModelTest
{
    [SetUp]
    public override async Task Setup()
    {
        await base.Setup();
        Model = Resolve<StatsViewModel>();
    }

    private StatsViewModel Model { get; set; }
}