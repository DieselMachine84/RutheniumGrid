using System;
using Avalonia;

namespace Ruthenium
{
    public class DataGridColumn : AvaloniaObject
    {
        public static readonly DirectProperty<DataGridColumn, string> FieldNameProperty =
            AvaloniaProperty.RegisterDirect<DataGridColumn, string>(nameof(FieldName),
                o => o.FieldName, (o, v) => o.FieldName = v);
        
        private string _fieldName = String.Empty;

        public string FieldName
        {
            get => _fieldName;
            set => SetAndRaise(FieldNameProperty, ref _fieldName, value);
        }
    }
}