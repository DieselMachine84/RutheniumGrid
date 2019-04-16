using Avalonia;
using Avalonia.Controls;

namespace Ruthenium
{
    public class DataGridCell : Control
    {
        private string Text { get; }

        private TextBlock TextBlock { get; set; }
        
        public int Row { get; }
        
        public int Column { get; }


        public DataGridCell(int row, int column, string text)
        {
            Row = row;
            Column = column;
            Text = text;
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
    }
}    