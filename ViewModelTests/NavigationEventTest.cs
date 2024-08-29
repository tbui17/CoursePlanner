using FluentAssertions;
using ViewModels.Domain;
using ViewModels.Events;
using ViewModels.Domain;
using ViewModelTests.TestData;
using ViewModelTests.TestSetup;

namespace ViewModelTests;

public class NavigationEventTest : BaseDbTest
{
    [Test]
    public async Task NavigationSubjectBehaviorTest()
    {
        var model = Resolve<TermViewModel>();
        var model2 = Resolve<TermViewModel>();
        model.Terms.Should().BeEmpty();
        var subj = Resolve<NavigationSubject>();
        subj.Publish(new NavigationEventArg(typeof(TestPage), 1));
        await Task.Delay(200);
        var model3 = Resolve<TermViewModel>();
        model3.Terms.Should().BeEmpty();
        model.Terms.Should().NotBeEmpty();
        model2.Terms.Should().NotBeEmpty();
        model3.Terms.Clear();
        model.Terms.Clear();
        model2.Terms.Clear();
        subj.Publish(new NavigationEventArg(typeof(TestPage), 2));
        await Task.Delay(200);
        model3.Terms.Should().NotBeEmpty();
        model.Terms.Should().NotBeEmpty();
        model2.Terms.Should().NotBeEmpty();

    }

    [Test]
    public async Task NavigationSubject_FiltersIrrelevantPage()
    {
        var model = Resolve<TermViewModel>();
        model.Terms.Should().BeEmpty();
        var subj = Resolve<NavigationSubject>();
        subj.Publish(new NavigationEventArg(typeof(TestPage2), 1));
        await Task.Delay(200);
        model.Terms.Should().BeEmpty();
        subj.Publish(new NavigationEventArg(typeof(TestPage), 1));
        await Task.Delay(200);
        model.Terms.Should().NotBeEmpty();

    }
}