using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Ruthenium.DataGrid
{
    public class DataGrid : TemplatedControl
    {
        internal const double GridLineThickness = 1.0;
        internal static SolidColorBrush LineBrush { get; } = new SolidColorBrush {Color = Colors.Black};

        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource;

        private List<Line> Border { get; } = new List<Line>();

        private GridPanel Panel { get; }

        internal DataController Controller { get; } = new DataController();

        public object ItemsSource
        {
            get => _itemsSource;
            set => SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
        }

        public List<Column> Columns { get; } = new List<Column>();


        static DataGrid()
        {
            ItemsSourceProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.ItemsSourceChanged(e));
        }

        public DataGrid()
        {
            //TODO: move to GridPanel?
            for (int i = 0; i < 4; i++)
            {
                var line = new Line() {Stroke = LineBrush, StrokeThickness = GridLineThickness};
                Border.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

            Panel = new GridPanel(this);
            LogicalChildren.Add(Panel);
            VisualChildren.Add(Panel);
        }

        protected void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Controller.SetItemsSource(ItemsSource);
            Panel.RecreateContent();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureBorder();
            Panel.Measure(new Size(availableSize.Width - 2.0 * GridLineThickness, availableSize.Height - 2.0 * GridLineThickness));
            return Panel.DesiredSize;

            void MeasureBorder()
            {
                Border[0].StartPoint = new Point(GridLineThickness / 2.0, 0.0);
                Border[0].EndPoint = new Point(GridLineThickness / 2.0, availableSize.Height);
                Border[1].StartPoint = new Point(availableSize.Width - GridLineThickness / 2.0, 0.0);
                Border[1].EndPoint = new Point(availableSize.Width - GridLineThickness / 2.0, availableSize.Height);
                Border[2].StartPoint = new Point(GridLineThickness, GridLineThickness / 2.0);
                Border[2].EndPoint = new Point(availableSize.Width - GridLineThickness, GridLineThickness / 2.0);
                Border[3].StartPoint = new Point(GridLineThickness, availableSize.Height - GridLineThickness / 2.0);
                Border[3].EndPoint = new Point(availableSize.Width - GridLineThickness, availableSize.Height - GridLineThickness / 2.0);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Panel.Arrange(new Rect(GridLineThickness, GridLineThickness,
                finalSize.Width - 2.0 * GridLineThickness, finalSize.Height - 2.0 * GridLineThickness));
            return finalSize;
        }
    }
}
