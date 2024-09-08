using AutoFixture;
using FakeItEasy;
using Moq;

namespace BaseTestSetup.Lib;

public static class FixtureExtensions
{
    public static Mock<T> FreezeMock<T>(this IFixture fixture) where T : class
    {
        var mock = fixture.Freeze<Mock<T>>();
        fixture.Inject(mock.Object);
        return mock;
    }

    public static Fake<T> FreezeFake<T>(this IFixture fixture) where T : class
    {
        var fake = fixture.Freeze<Fake<T>>();
        fixture.Inject(fake.FakedObject);
        return fake;
    }

    public static Mock<T> CreateMock<T>(this IFixture fixture) where T : class => fixture.Create<Mock<T>>();
}