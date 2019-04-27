using System;
using Avalonia;
using Avalonia.Controls;

namespace Ruthenium.DataGrid
{
    public class GridCell : Control
    {
        private IControl Control { get; }

        internal int Row { get; set; } = -1;

        internal int VisibleRow { get; set; } = -1;

        public GridColumn Column { get; }


        public GridCell(GridColumn column)
        {
            Column = column;
            Control = column.CreateControl();
            LogicalChildren.Add(Control);
            VisualChildren.Add(Control);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Control.Measure(availableSize);
            return Control.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Control.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public override string ToString()
        {
            return $"Row: {Row}, Column: {Column.Index}, Text: {DataContext?.ToString() ?? String.Empty}";
        }
    }
}