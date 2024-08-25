using Lib.Utils;

namespace LibTests;

using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using System.Collections.Generic;

public class GridStateTests
{
    [Test]
    public void Properties_Initial_ShouldBeFalsy()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var elements = new List<int>();


        IAutoGridState gridState = new AutoGridState
        {
            Columns = 2,
            ChildCount = () => elements.Count
        };

        using var _ = new AssertionScope();

        var test = new GridPropertyTest(0, 0, 0, 0, 0, false);

        test.AssertGridState(gridState);
    }

    [TestCaseSource(nameof(GridStateTestCases))]
    public void Properties_ShouldBeCongruentWithElementCount(GridTestCase testCase)
    {
        var elements = new List<int>();
        var gridState = new AutoGridState
        {
            Columns = testCase.Columns,
            ChildCount = () => elements.Count
        };

        using var scope = new AssertionScope();

        foreach (var test in testCase.Tests)
        {
            elements.Add(1);
            test.AssertGridState(gridState);
        }
    }

    private static IEnumerable<TestCaseData> GridStateTestCases()
    {
        var zeroColumnTestData = new GridTestCase
        {
            Columns = 0,
            Tests =
            [
                new(Count: 1, Index: 0, Column: 0, Row: 0, Rows: 1, ShouldAddRowDefinition: true),
                new(Count: 2, Index: 1, Column: 0, Row: 1, Rows: 2, ShouldAddRowDefinition: true),
                new(Count: 3, Index: 2, Column: 0, Row: 2, Rows: 3, ShouldAddRowDefinition: true),
                new(Count: 4, Index: 3, Column: 0, Row: 3, Rows: 4, ShouldAddRowDefinition: true),
                new(Count: 5, Index: 4, Column: 0, Row: 4, Rows: 5, ShouldAddRowDefinition: true),
                new(Count: 6, Index: 5, Column: 0, Row: 5, Rows: 6, ShouldAddRowDefinition: true),
                new(Count: 7, Index: 6, Column: 0, Row: 6, Rows: 7, ShouldAddRowDefinition: true),
                new(Count: 8, Index: 7, Column: 0, Row: 7, Rows: 8, ShouldAddRowDefinition: true),
                new(Count: 9, Index: 8, Column: 0, Row: 8, Rows: 9, ShouldAddRowDefinition: true),
            ]
        };
        IEnumerable<GridTestCase> cases =
        [
            zeroColumnTestData,
            new() { Columns = 1, Tests = zeroColumnTestData.Tests },
            new()
            {
                Columns = 2,
                Tests =
                [
                    new(Count: 1, Index: 0, Column: 0, Row: 0, Rows: 1, ShouldAddRowDefinition: true),
                    new(Count: 2, Index: 1, Column: 1, Row: 0, Rows: 1, ShouldAddRowDefinition: false),
                    new(Count: 3, Index: 2, Column: 0, Row: 1, Rows: 2, ShouldAddRowDefinition: true),
                    new(Count: 4, Index: 3, Column: 1, Row: 1, Rows: 2, ShouldAddRowDefinition: false),
                    new(Count: 5, Index: 4, Column: 0, Row: 2, Rows: 3, ShouldAddRowDefinition: true),
                    new(Count: 6, Index: 5, Column: 1, Row: 2, Rows: 3, ShouldAddRowDefinition: false),
                    new(Count: 7, Index: 6, Column: 0, Row: 3, Rows: 4, ShouldAddRowDefinition: true),
                    new(Count: 8, Index: 7, Column: 1, Row: 3, Rows: 4, ShouldAddRowDefinition: false),
                    new(Count: 9, Index: 8, Column: 0, Row: 4, Rows: 5, ShouldAddRowDefinition: true)
                ]
            },
            new()
            {
                Columns = 3,
                Tests =
                [
                    new(Count: 1, Index: 0, Column: 0, Row: 0, Rows: 1, ShouldAddRowDefinition: true),
                    new(Count: 2, Index: 1, Column: 1, Row: 0, Rows: 1, ShouldAddRowDefinition: false),
                    new(Count: 3, Index: 2, Column: 2, Row: 0, Rows: 1, ShouldAddRowDefinition: false),
                    new(Count: 4, Index: 3, Column: 0, Row: 1, Rows: 2, ShouldAddRowDefinition: true),
                    new(Count: 5, Index: 4, Column: 1, Row: 1, Rows: 2, ShouldAddRowDefinition: false),
                    new(Count: 6, Index: 5, Column: 2, Row: 1, Rows: 2, ShouldAddRowDefinition: false),
                    new(Count: 7, Index: 6, Column: 0, Row: 2, Rows: 3, ShouldAddRowDefinition: true),
                    new(Count: 8, Index: 7, Column: 1, Row: 2, Rows: 3, ShouldAddRowDefinition: false),
                    new(Count: 9, Index: 8, Column: 2, Row: 2, Rows: 3, ShouldAddRowDefinition: false)
                ]
            }
        ];

        return cases.Select(x => new TestCaseData(x));
    }
}

public record GridPropertyTest(int Count, int Index, int Column, int Row, int Rows, bool ShouldAddRowDefinition)
    : IAutoGridState
{
    public void AssertGridState(IAutoGridState gridState)
    {
        using var scope = new AssertionScope();

        IAutoGridState self = this;

        gridState
            .Should()
            .BeEquivalentTo(self);

        if (scope.HasFailures())
        {
            scope.FailWith("Expected: {0}\nReceived: {1}", self, gridState);
        }
    }
}

public record GridTestCase
{
    public required int Columns { get; set; }
    public ICollection<GridPropertyTest> Tests { get; set; } = [];
}