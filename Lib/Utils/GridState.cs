namespace Lib.Utils;

public abstract class GridState(Func<int> childCount)
{
    private readonly int _columns = 2;

    public int Columns
    {
        get => _columns;
        init => _columns = Math.Max(value, 1);
    }

    public int Count => childCount();

    public int Index => Math.Max(0, Count - 1);

    public int Row =>
        Count <= Columns
            ? 0
            : Index / Columns;

    public int Column =>
        Columns <= 1 || Count <= 1
            ? 0
            : Index % Columns;

    public int Rows =>
        Count <= 0
            ? 0
            : Row + 1;
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

public class AutoGridState(Func<int> childCount) : GridState(childCount), IAutoGridState
{
    public bool ShouldAddRowDefinition => Column is 0 && Count > 0;
}