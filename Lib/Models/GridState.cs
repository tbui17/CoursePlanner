namespace Lib.Models;

public abstract class GridState
{
    public required Func<int> ChildCount { get; init; }
    public int Columns { get; init; } = 2;

    protected int Index => Count - 1;

    public int Row =>
        Count <= Columns
            ? 0
            : Index / Columns;

    protected int Count => ChildCount();

    public int Column =>
        Columns <= 1 || Count <= 1
            ? 0
            : Index % Columns;

    public int Rows =>
        Count switch
        {
            <= 0 => 0,
            _ => Row + 1
        };
}

public class AutoGridState : GridState
{
    public bool ShouldAddRowDefinition => Column is 0 && Count > 0;
}