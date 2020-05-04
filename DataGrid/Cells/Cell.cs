using System;
using Avalonia;
using Avalonia.Controls;

namespace Ruthenium.DataGrid
{
    public class Cell : Control
    {
        private IControl Control { get; set; }

        internal int Row { get; set; } = -1;

        internal int VisibleRow { get; set; } = -1;

        public Column Column { get; }

        static Cell()
        {
            ClipToBoundsProperty.OverrideDefaultValue<Cell>(true);
        }

        public Cell(Column column)
        {
            Column = column;
            if (Column.DynamicCreateControlsForCells)
            {
                DataContextChanged += OnDataContextChanged;
            }
            else
            {
                Control = Column.CreateControl();
                LogicalChildren.Add(Control);
                VisualChildren.Add(Control);
            }
        }

        private void OnDataContextChanged(object sender, EventArgs e)
        {
            LogicalChildren.Remove(Control);
            VisualChildren.Remove(Control);
            Control = Column.DynamicCreateControl(this);
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