using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Ruthenium.DataGrid
{
    public class ColumnHeader : Control
    {
        internal Column Column { get; }
        private TextBlock TextBlock { get; }
        private SolidColorBrush BackgroundBrush { get; } = new SolidColorBrush {Color = Colors.Gray};
        
        public ColumnHeader(Column column)
        {
            Column = column;
            TextBlock = new TextBlock {Margin = new Thickness(1.0)};
            TextBlock.Text = Column.FieldName;
            LogicalChildren.Add(TextBlock);
            VisualChildren.Add(TextBlock);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            TextBlock.Measure(availableSize);
            return TextBlock.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            TextBlock.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public override void Render(DrawingContext context)
        {
            context.FillRectangle(BackgroundBrush, new Rect(Bounds.Size));
            base.Render(context);
        }
    }
}