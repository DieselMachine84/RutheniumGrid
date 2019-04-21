using System;
using Avalonia;

namespace Ruthenium.DataGrid
{
    public class GridColumn : AvaloniaObject
    {
        public static readonly DirectProperty<GridColumn, string> FieldNameProperty =
            AvaloniaProperty.RegisterDirect<GridColumn, string>(nameof(FieldName),
                o => o.FieldName, (o, v) => o.FieldName = v);
        
        private string _fieldName = String.Empty;

        public string FieldName
        {
            get => _fieldName;
            set => SetAndRaise(FieldNameProperty, ref _fieldName, value);
        }
    }
}