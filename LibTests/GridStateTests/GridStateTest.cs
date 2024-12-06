using FluentAssertions.Execution;
using Lib.Utils;

namespace LibTests.GridStateTests;

public class GridStateTests
{
    [Test]
    public void Properties_Columns2Count3_MatchesSnapshot()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var elements = new List<int>();


        IAutoGridState gridState = new AutoGridState(() => elements.Count)
        {
            Columns = 2,
        };

        // simulate "grid" adding 3 elements
        elements.Add(1);
        elements.Add(1);
        elements.Add(1);

        new AutoGridStateSnapshot(
            Count: 3,
            Index: 2,
            Column: 0,
            Row: 1,
            Rows: 2,
            ShouldAddRowDefinition: true
        ).AssertEquivalentToSelf(gridState);
    }

    [Test]
    public void Properties_Initial_ShouldBeFalsy()
    {
        // ReSharper disable once CollectionNeverUpdated.Local
        var elements = new List<int>();


        IAutoGridState gridState = new AutoGridState(() => elements.Count)
        {
            Columns = 2,
        };

        using var _ = new AssertionScope();

        var snap = new AutoGridStateSnapshot(0,
            0,
            0,
            0,
            0,
            false
        );

        snap.AssertEquivalentToSelf(gridState);
    }

    [TestCaseSource(typeof(GridTestCase), nameof(GridTestCase.GridStateTestCases))]
    public void Properties_RepeatingSequence_MatchesSnapshot(GridTestCase testCase)
    {
        var elements = new List<int>();
        var gridState = new AutoGridState(() => elements.Count)
        {
            Columns = testCase.Columns,
        };

        using var scope = new AssertionScope();

        foreach (var snap in testCase.Snapshots)
        {
            elements.Add(1);
            snap.AssertEquivalentToSelf(gridState);
        }
    }
}