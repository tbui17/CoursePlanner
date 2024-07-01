using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;


namespace TestSuite;

public class GridStateTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void GridState_Should_Be_Congruent_With_Element_Count()
    {
        var count = 0;


        // ReSharper disable AccessToModifiedClosure
        var gridState = new AutoGridState { Columns = 2, ChildCount = () => count };

        using var _ = new AssertionScope();
        
        gridState.AssertGridState(0,0,false,0);

        
        List<(int col, int row, bool shouldAddRow, int rows)> expected =
        [
            (0, 0, true,1), // one item, therefore at time of access, should specify handler to assign row=0, col = 0 to element. first row should be added here.
            (1, 0, false,1), // 2 items, assign to second column, no change in row.
            (0, 1, true,2), // 3 items, moved to next row at the starting column because it exceeds max column count of 2
            (1, 1, false,2),
            (0, 2, true,3),
            (1, 2, false,3),
            (0, 3, true,4),
            (1, 3, false,4 ),
            (0, 4, true, 5)
        ];

        foreach (var (col, row, shouldAddRow, rows) in expected)
        {
            count++;
            gridState.AssertGridState(col, row, shouldAddRow,rows);
        }
    }
}

public static class GridStateTestExtensions
{
    public static void AssertGridState(this AutoGridState gridState, int column, int row, bool shouldAddRow, int rows)
    {
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