using Lib.Models;
using Lib.Utils;

namespace CoursePlanner.Controls;

public partial class AutoGrid
{
    private const int DefaultColumnCount = 2;

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

    private void AddRowDefinition() => RowDefinitions.Add(new RowDefinition(DefaultRowLength));

    private void AddColumnDefinition() => ColumnDefinitions.Add(new ColumnDefinition(DefaultColumnLength));

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
    public static readonly BindableProperty DefaultColumnLengthProperty =
        BindableProperty.Create(
            nameof(DefaultColumnLength),
            typeof(GridLength),
            typeof(AutoGrid)
        );

    public GridLength DefaultColumnLength
    {
        get => GetGridLength(DefaultColumnLengthProperty);
        set => SetValue(DefaultColumnLengthProperty, value);
    }

    private GridLength GetGridLength(BindableProperty bindable) =>
        (GridLength)GetValue(bindable) switch
        {
            { Value: <= 0 } => GridLength.Star,
            var x => x
        };


    public static readonly BindableProperty DefaultRowLengthProperty =
        BindableProperty.Create(
            nameof(DefaultRowLength),
            typeof(GridLength),
            typeof(AutoGrid)
        );

    public GridLength DefaultRowLength
    {
        get => GetGridLength(DefaultColumnLengthProperty);
        set => SetValue(DefaultRowLengthProperty, value);
    }

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