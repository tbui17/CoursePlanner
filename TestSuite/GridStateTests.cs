using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;


namespace TestSuite;

public class GridStateTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Beginning_Grid_State()
    {
        var count = 0;


        var gridState = new AutoGridState { Columns = 2, ChildCount = () => count };

        using var _ = new AssertionScope();

        gridState.AssertGridState(new(0, 0, false, 0));
    }


    [Test, TestCaseSource(nameof(GridStateTestCases))]
    public void GridState_Should_Be_Congruent_With_Element_Count(
        int columns,
        List<GridTestDataItem> expected
    )
    {
        var elements = new List<int>();
        var gridState = new AutoGridState { Columns = columns, ChildCount = () => elements.Count };

        using var _ = new AssertionScope();

        foreach (var item in expected)
        {
            elements.Add(1);
            gridState.AssertGridState(item);
        }
    }

    private static IEnumerable<TestCaseData> GridStateTestCases()
    {
        List<GridTestDataItem> oneColumnCaseData =
        [
            new(0, 0, true, 1),
            new(0, 1, true, 2),
            new(0, 2, true, 3),
            new(0, 3, true, 4),
            new(0, 4, true, 5),
            new(0, 5, true, 6),
            new(0, 6, true, 7),
            new(0, 7, true, 8),
            new(0, 8, true, 9)
        ];

        GridTestCaseData[] data =
        [
            new(0, oneColumnCaseData),
            new(1, oneColumnCaseData),
            new(2,
                [
                    new(0, 0, true, 1),
                    new(1, 0, false, 1),

                    new(0, 1, true, 2),
                    new(1, 1, false, 2),

                    new(0, 2, true, 3),
                    new(1, 2, false, 3),

                    new(0, 3, true, 4),
                    new(1, 3, false, 4),

                    new(0, 4, true, 5)
                ]
            ),
            new(3,
                [
                    new(0, 0, true, 1),
                    new(1, 0, false, 1),
                    new(2, 0, false, 1),

                    new(0, 1, true, 2),
                    new(1, 1, false, 2),
                    new(2, 1, false, 2),

                    new(0, 2, true, 3),
                    new(1, 2, false, 3),
                    new(2, 2, false, 3)
                ]
            ),
        ];
        return data
           .Select(TestCaseData (x) => x)
           .Select(x => x.SetName($"{x.Arguments[0]} columns"));
    }
}

public record GridTestCaseData(int Columns, List<GridTestDataItem> Expected)
{
    
    public static implicit operator TestCaseData(GridTestCaseData data) => new(data.Columns, data.Expected);
}

public record GridTestDataItem(int Col, int Row, bool ShouldAddRow, int Rows);

public static class GridStateTestExtensions
{
    public static void AssertGridState(this AutoGridState gridState, GridTestDataItem item)
    {
        var (column, row, shouldAddRow, rows) = item;
        gridState
           .Column
           .Should()
           .Be(column);
        gridState
           .Row
           .Should()
           .Be(row);
        gridState
           .ShouldAddRowDefinition
           .Should()
           .Be(shouldAddRow);
        gridState
           .Rows
           .Should()
           .Be(rows);
    }
}