using FluentAssertions;
using Lib.Utils;

namespace LibTests.Utils;

[TestFixture]
[TestOf(typeof(NamespaceData))]
public class NamespaceDataTest
{
    [Test]
    public void FromNameOfExpression_ParsesFullExpression()
    {
        NamespaceData.FromNameofExpression(nameof(LibTests.Utils))
            .FullNamespace.Should()
            .Be("LibTests.Utils");
    }

    [Test]
    public void ToNamespaceData_ParsesFullExpression()
    {
        nameof(LibTests.Utils)
            .ToNamespaceData()
            .FullNamespace.Should()
            .Be("LibTests.Utils");
    }


    [Test]
    public void ToNamespaceString_ParsesFullExpression()
    {
        nameof(LibTests.Utils)
            .ToNamespaceString()
            .Should()
            .Be("LibTests.Utils");
    }
}