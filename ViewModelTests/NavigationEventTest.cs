using FluentAssertions;
using ViewModels.Events;
using ViewModels.PageViewModels;
using ViewModelTests.TestData;

namespace ViewModelTests;

public class NavigationEventTest
{
    [Test]
    public async Task METHOD()
    {
        var model = Resolve<TermViewModel>();
        model.Terms.Should().BeEmpty();
        var subj = Resolve<NavigationSubject>();
        subj.Publish(new NavigationEventArg(new TestPage(), 1));
        await Task.Delay(200);
        model.Terms.Should().NotBeEmpty();
    }
}