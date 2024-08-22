using Lib.Utils;

namespace CoursePlanner.Controls;

public partial class AutoGrid
{
    private const int DefaultColumnCount = 2;

    public GridLength DefaultHeight { get; init; } = GridLength.Star;
    public GridLength DefaultWidth { get; init; } = GridLength.Star;

    public AutoGrid()
    {
        InitializeComponent();
        InitializeColumnDefinitions();
        State = CreateAutoGridState();
        ChildAdded += OnChildAdded;
    }

    private AutoGridState State { get; set; }

    private void InitializeColumnDefinitions()
    {
        if (ColumnDefinitions.Count == 0) ColumnCount.Times(AddColumnDefinition);
    }

    private void AddRowDefinition() => RowDefinitions.Add(new RowDefinition(DefaultHeight));

    private void AddColumnDefinition() => ColumnDefinitions.Add(new ColumnDefinition(DefaultWidth));

    private AutoGridState CreateAutoGridState() =>
        new() { Columns = ColumnDefinitions.Count, ChildCount = () => Children.Count };


    private void OnChildAdded(object? sender, ElementEventArgs e)
    {
        if (State.ShouldAddRowDefinition) AddRowDefinition();

        var view = (View)e.Element;
        Grid.SetRow(view, State.Row);
        Grid.SetColumn(view, State.Column);
    }
}

public partial class AutoGrid
{


    public static readonly BindableProperty ColumnCountProperty =
        BindableProperty.Create(
            nameof(ColumnCount),
            typeof(int),
            typeof(AutoGrid)
        );

    public int ColumnCount
    {
        get =>
            (int)GetValue(ColumnCountProperty) switch
            {
                <= 0 => DefaultColumnCount,
                var x => x
            };
        set => SetValue(ColumnCountProperty, value);
    }
}