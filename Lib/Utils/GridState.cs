namespace Lib.Utils;

public abstract class GridState(Func<int> childCount)
{
    private readonly int _columns = 2;

    public int Columns
    {
        get => _columns;
        init => _columns = Math.Max(value, 1); // avoid defining 0 column behavior
    }

    public int Count => childCount();

    public int Index => Math.Max(0, Count - 1); // prevent index errors in unforeseen scenarios

    public int Row =>
        Count <= Columns // prevent division by 0 and other reasons, don't remember
            ? 0
            : Index / Columns; // repeating sequence

    public int Column =>
        // prevent division by 0; when consumer only wants 1 column max we're always at the first; when there's <=1 element we're always at the first
        Columns <= 1 || Count <= 1
            ? 0
            : Index % Columns; // repeating sequence

    public int Rows =>
        Count <= 0 // if we have no elements in the grid there is no structure of any kind
            ? 0
            : Row + 1;
}

public class AutoGridState(Func<int> childCount) : GridState(childCount), IAutoGridState
{
    public bool ShouldAddRowDefinition =>
        Column is 0 &&
        Count > 0; // when we arrive at a new row we're at the first column; we don't add if there is no structure in place (no elements)
}

public interface IAutoGridState
{
    int Rows { get; }
    int Row { get; }
    int Column { get; }
    int Count { get; }
    int Index { get; }
    bool ShouldAddRowDefinition { get; }
}