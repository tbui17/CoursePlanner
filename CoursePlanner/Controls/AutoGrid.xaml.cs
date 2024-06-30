using Lib.Models;

namespace CoursePlanner.Controls;

using static CommunityToolkit.Maui.Markup.GridRowsColumns;

public partial class AutoGrid
{
    public AutoGrid()
    {
        InitializeComponent();
        if (ColumnDefinitions.Count == 0)
        {
            AddColumnDefinition();
            AddColumnDefinition();
            
        }

        AddRowDefinition();
        State = new() { Columns = ColumnDefinitions.Count, ChildCount = () => Children.Count };
        ChildAdded += OnChildAdded;
    }

    private GridState State { get; set; }

    private void AddRowDefinition()
    {
        RowDefinitions.Add(new RowDefinition(Auto));
    }
    
    private void AddColumnDefinition()
    {
        ColumnDefinitions.Add(new ColumnDefinition(Star));
    }

    private void OnChildAdded(object? sender, ElementEventArgs e)
    {
        var view = (View)e.Element;
        if (State.ShouldAddRowDefinition) AddRowDefinition();
        Grid.SetRow(view, State.Row);
        Grid.SetColumn(view, State.Column);
    }
}
