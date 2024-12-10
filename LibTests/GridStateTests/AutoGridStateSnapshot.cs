using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Utils;

namespace LibTests.GridStateTests;

public record AutoGridStateSnapshot(int Count, int Index, int Column, int Row, int Rows, bool ShouldAddRowDefinition)
    : IAutoGridState
{
    public void AssertEquivalentToSelf(IAutoGridState gridState)
    {
        using var scope = new AssertionScope();

        IAutoGridState self = this;

        // object equivalence against IAutoGridState interface
        gridState
            .Should()
            .BeEquivalentTo(self);

        if (scope.HasFailures())
        {
            scope.FailWith("Expected: {0}\nReceived: {1}", self, gridState);
        }
    }
}