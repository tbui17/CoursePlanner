using FluentAssertions;
using JetBrains.Annotations;

namespace BuildTests.Utils;

public static class TestSourceTemplates
{
    [SourceTemplate]
    public static void ShouldNotBeNull(this object? obj)
    {
        obj.Should().NotBeNull();
    }
}