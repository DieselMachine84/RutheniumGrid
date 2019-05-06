using System;
using Avalonia;
using Avalonia.Controls;

namespace Ruthenium.DataGrid
{
    public abstract class Column : AvaloniaObject
    {
        public static readonly DirectProperty<Column, string> FieldNameProperty =
            AvaloniaProperty.RegisterDirect<Column, string>(nameof(FieldName),
                o => o.FieldName, (o, v) => o.FieldName = v);

        public static readonly DirectProperty<Column, double> WidthProperty =
            AvaloniaProperty.RegisterDirect<Column, double>(nameof(Width),
                o => o.Width, (o, v) => o.Width = v);

        private string _fieldName = String.Empty;
        private double _width; 
        
        protected internal virtual bool DynamicCreateControlsForCells => false;

        public string FieldName
        {
            get => _fieldName;
            set => SetAndRaise(FieldNameProperty, ref _fieldName, value);
        }

        public double Width
        {
            get => _width;
            set => SetAndRaise(WidthProperty, ref _width, value);
        }

        public int Index { get; internal set; }

        public ColumnHeader CreateColumnHeader()
        {
            return new ColumnHeader(this);
        }

        public abstract IControl CreateControl();

        public virtual IControl DynamicCreateControl(Cell cell) => null;
    }
}