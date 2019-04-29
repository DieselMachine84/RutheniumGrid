using System;
using Avalonia;
using Avalonia.Controls;

namespace Ruthenium.DataGrid
{
    public abstract class GridColumn : AvaloniaObject
    {
        public static readonly DirectProperty<GridColumn, string> FieldNameProperty =
            AvaloniaProperty.RegisterDirect<GridColumn, string>(nameof(FieldName),
                o => o.FieldName, (o, v) => o.FieldName = v);

        private string _fieldName = String.Empty;
        
        protected internal virtual bool DynamicCreateControlsForCells => false;

        public string FieldName
        {
            get => _fieldName;
            set => SetAndRaise(FieldNameProperty, ref _fieldName, value);
        }

        public int Index { get; internal set; }

        public abstract IControl CreateControl();

        public virtual IControl DynamicCreateControl(GridCell cell) => null;
    }
}