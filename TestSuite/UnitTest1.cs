using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;

namespace TestSuite;

public class Tests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void GridState_Should_Be_Congruent_With_Element_Count()
    {
        var elements = new List<int>();
        var gridState = new GridState { Columns = 2, ChildCount = ChildCount };
        
        using var _ = new AssertionScope();
        TestGrid(0, 0, false);


        List<(int col, int row, bool shouldAddRow)> expected =
        [
            (0, 0, false),
            (1, 0, false),
            (0, 1, true),
            (1, 1, false),
            (0, 2, true),
            (1, 2, false),
            (0, 3, true),
            (1, 3, false),
            (0, 4, true)
        ];

        foreach (var (col, row, shouldAddRow) in expected)
        {
            elements.Add(1);
            TestGrid(col, row, shouldAddRow);
        }

        return;
        int ChildCount() => elements.Count;

        void TestGrid(int column, int row, bool shouldAddRow)
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
        }
    }
}