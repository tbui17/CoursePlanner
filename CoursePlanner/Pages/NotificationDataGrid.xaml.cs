using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Maui.DataGrid;

namespace CoursePlanner.Pages;

public partial class NotificationDataGrid : ContentPage
{
    public NotificationDataGrid()
    {
        new DataGridColumn() { };
        new DataGrid()
        {
            Columns = [

            ]

        };
        InitializeComponent();
    }
}