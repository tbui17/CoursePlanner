namespace Lib.Utils;

public abstract record GridState
{
    public required Func<int> ChildCount { get; init; }

    private readonly int _columns = 2;

    public int Columns
    {
        get => _columns;
        init => _columns = Math.Max(value, 1);
    }

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

public interface IAutoGridState
{
    int Rows { get; }
    int Row { get; }
    int Column { get; }
    bool ShouldAddRowDefinition { get; }
}

public record AutoGridState : GridState, IAutoGridState
{
    public bool ShouldAddRowDefinition => Column is 0 && Count > 0;
}