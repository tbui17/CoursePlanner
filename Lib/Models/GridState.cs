namespace Lib.Models;

public record GridState
{
    public required Func<int> ChildCount { get; init; }
    public int Columns { get; init; } = 2;

    private int Index => Count - 1;

    public int Row =>
        Count <= Columns
            ? 0
            : Index / Columns;

    private int Count => ChildCount();

    public int Column =>
        Columns <= 1 || Count <= 1
            ? 0
            : Index % Columns;

    public bool ShouldAddRowDefinition => Count > 1 && Column == 0;
}