using System;
using Avalonia;
using Avalonia.Controls;

namespace Ruthenium.DataGrid
{
    public class Cell : Panel
    {
        internal static int MeasureCount { get; set; }
        private IControl Control { get; set; }

        internal int Row { get; set; } = -1;

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
                Children.Add(Control);
            }
        }

        private void OnDataContextChanged(object sender, EventArgs e)
        {
            Children.Remove(Control);
            Control = Column.DynamicCreateControl(this);
            Children.Add(Control);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            unchecked { MeasureCount++; }
            //TODO performance: setting Width and Height forces two Control.Measure passes
            Width = Column.Width;
            Control.Measure(availableSize);
            Height = Control.DesiredSize.Height;
            return new Size(Width, Height);
        }

        public override string ToString()
        {
            return $"Row: {Row}, Column: {Column.Index}, Text: {DataContext?.ToString() ?? String.Empty}, IsVisible {IsVisible}";
        }
    }
}
