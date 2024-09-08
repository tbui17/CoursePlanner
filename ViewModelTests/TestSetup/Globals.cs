using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;

namespace ViewModelTests.TestSetup;

public static class Globals
{
    public static IFixture CreateFixture()
    {
        var fixture = new Fixture()
            .Customize(new AutoMoqCustomization());
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    public static Mock<T> CreateMock<T>() where T : class => CreateFixture().Create<Mock<T>>();
}