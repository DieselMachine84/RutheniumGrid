using Avalonia;
using Avalonia.Controls;

namespace Ruthenium
{
    public class DataGridCell : Control
    {
        private TextBlock TextBlock { get; set; }

        internal string Text { get; set; }

        public int Row { get; internal set; } = -1;

        public int Column { get; }


        public DataGridCell(int column)
        {
            Column = column;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (TextBlock == null)
                return Size.Empty;
            
            TextBlock.Measure(availableSize);
            return TextBlock.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (TextBlock == null)
                return Size.Empty;
            
            TextBlock.Arrange(new Rect(finalSize));
            return finalSize;
        }

        //TODO why this code is within ApplyTemplate?
        public override void ApplyTemplate()
        {
            TextBlock = new TextBlock() {Text = Text, Margin = new Thickness(1.0)};
            LogicalChildren.Clear();
            LogicalChildren.Add(TextBlock);
            VisualChildren.Clear();
            VisualChildren.Add(TextBlock);
        }

        public override string ToString()
        {
            return $"Row: {Row}, Column: {Column}, Text: {Text}";
        }
    }
}