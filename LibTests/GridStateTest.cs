using Lib.Utils;

namespace LibTests;

using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
using NUnit.Framework;
using System.Collections.Generic;

public class GridStateTests
{
    [Test]
    public void Properties_Initial_ShouldBeFalsy()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var elements = new List<int>();


        var gridState = new AutoGridState
        {
            Columns = 2,
            ChildCount = () => elements.Count
        };

        using var _ = new AssertionScope();

        var test = new GridPropertyTest(0, 0, false, 0);

        test.AssertGridState(gridState);
    }

    [TestCaseSource(nameof(GridStateTestCases))]
    public void Properties_ShouldBeCongruentWithElementCount(int columns, List<GridPropertyTest> tests)
    {
        var elements = new List<int>();
        var gridState = new AutoGridState
        {
            Columns = columns,
            ChildCount = () => elements.Count
        };

        using var scope = new AssertionScope();

        foreach (var test in tests)
        {
            elements.Add(1);
            test.AssertGridState(gridState);
        }
    }

    public static IEnumerable<TestCaseData> GridStateTestCases()
    {
        List<GridPropertyTest> oneColumnCaseData = new List<GridPropertyTest>
        {
            new(0, 0, true, 1),
            new(0, 1, true, 2),
            new(0, 2, true, 3),
            new(0, 3, true, 4),
            new(0, 4, true, 5),
            new(0, 5, true, 6),
            new(0, 6, true, 7),
            new(0, 7, true, 8),
            new(0, 8, true, 9)
        };

        yield return new TestCaseData(0, oneColumnCaseData);
        yield return new TestCaseData(1, oneColumnCaseData);

        yield return new TestCaseData(2, new List<GridPropertyTest>
            {
                new(0, 0, true, 1),
                new(1, 0, false, 1),
                new(0, 1, true, 2),
                new(1, 1, false, 2),
                new(0, 2, true, 3),
                new(1, 2, false, 3),
                new(0, 3, true, 4),
                new(1, 3, false, 4),
                new(0, 4, true, 5)
            }
        );

        yield return new TestCaseData(3, new List<GridPropertyTest>
            {
                new(0, 0, true, 1),
                new(1, 0, false, 1),
                new(2, 0, false, 1),
                new(0, 1, true, 2),
                new(1, 1, false, 2),
                new(2, 1, false, 2),
                new(0, 2, true, 3),
                new(1, 2, false, 3),
                new(2, 2, false, 3)
            }
        );
    }
}

public record GridPropertyTest(int Column, int Row, bool ShouldAddRowDefinition, int Rows) : IAutoGridState
{
    public void AssertGridState(AutoGridState gridState)
    {
        using var scope = new AssertionScope();
        var data = new
        {
            Id = Guid
               .NewGuid()
               .ToString(),
            GridState = gridState.ToString(),
            TestData = ToString(),
        };

        scope.AddReportable(data.Id, data.ToString);

        gridState
           .Should()
           .BeEquivalentTo(this);
    }
}