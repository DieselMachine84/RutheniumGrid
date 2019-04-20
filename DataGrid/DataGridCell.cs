using Avalonia;
using Avalonia.Controls;

namespace Ruthenium
{
    public class DataGridCell : Control
    {
        private TextBlock TextBlock { get; set; }

        internal string Text
        {
            get => TextBlock.Text;
            set => TextBlock.Text = value;
        }

        internal int Row { get; set; } = -1;

        internal int VisibleRow { get; set; } = -1;

        public int Column { get; }


        public DataGridCell(int column)
        {
            Column = column;
            TextBlock = new TextBlock() {Margin = new Thickness(1.0)};
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