using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Models;
namespace TestSuite;

public class GridStateTests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Starting_State_Should_Be_All_Falsy()
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


    [Test, TestCaseSource(nameof(GridStateTestCases))]
    public void GridState_Should_Be_Congruent_With_Element_Count(
        int columns,
        List<GridPropertyTest> tests
    )
    {
        var elements = new List<int>();
        var gridState = new AutoGridState
        {
            Columns = columns,
            ChildCount = () => elements.Count
        };

        using var _ = new AssertionScope();

        foreach (var test in tests)
        {
            elements.Add(1);
            test.AssertGridState(gridState);
        }
    }

    private static IEnumerable<TestCaseData> GridStateTestCases()
    {
        List<GridPropertyTest> oneColumnCaseData =
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

        GridTestCase[] data =
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

public record GridTestCase(int Columns, List<GridPropertyTest> Tests)
{
    public static implicit operator TestCaseData(GridTestCase data) => new(data.Columns, data.Tests);
}

public record GridPropertyTest(int Column, int Row, bool ShouldAddRowDefinition, int Rows)
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
           .Column
           .Should()
           .Be(Column, ToString());
        gridState
           .Row
           .Should()
           .Be(Row);
        gridState
           .ShouldAddRowDefinition
           .Should()
           .Be(ShouldAddRowDefinition);
        gridState
           .Rows
           .Should()
           .Be(Rows);
    }
}