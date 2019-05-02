using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace Ruthenium.DataGrid
{
    public class CellsPanel : Control, ILogicalScrollable
    {
        private const double GridLineThickness = 1.0;
        
        private Vector _offset = new Vector(0.0, 0.0);
        
        private DataController Controller { get; } = new DataController();

        internal List<GridColumn> Columns { get; } = new List<GridColumn>();

        private GridCells Cells { get; set; }


        private List<double> ColumnWidths { get; } = new List<double>();

        private List<double> AccumulatedColumnWidths { get; } = new List<double>();

        private List<double> RowHeights { get; } = new List<double>();

        private List<double> AccumulatedRowHeights { get; } = new List<double>();

        private double AccumulatedRowHeightsStart { get; set; }

        private SolidColorBrush LineBrush { get; }


        private List<Line> ColumnLines { get; } = new List<Line>();

        private List<Line> RowLines { get; } = new List<Line>();


        public Size Extent => new Size(DesiredSize.Width, Controller.Count);

        public Size Viewport { get; private set; } = Size.Empty;

        public Vector Offset
        {
            get => _offset;
            set
            {
                _offset = value;
                InvalidateMeasure();
            }
        }


        public CellsPanel()
        {
            LineBrush = new SolidColorBrush();
            LineBrush.Color = Colors.Black;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Columns.Count == 0)
                return Size.Empty;

            ZeroColumnRowSizes();

            double offsetFloor = Math.Floor(Offset.Y);
            int firstRow = Convert.ToInt32(offsetFloor);
            int row = firstRow;
            int cellIndex = Cells.GetFirstRowCellIndex(row);
            double viewportRowsHeight = 0.0;
            double firstVisibleRowVisiblePart = 1.0;
            double lastVisibleRowVisiblePart = 1.0;

            //TODO very big one and two rows
            while (viewportRowsHeight < availableSize.Height && row < Controller.Count)
            {
                int visibleRowIndex = row - firstRow;

                for (int column = 0; column < Columns.Count; column++)
                {
                    bool cellNeedsMeasure = false;
                    var cell = Cells[cellIndex + column];
                    cell.VisibleRow = visibleRowIndex;
                    if (cell.Row != row)
                    {
                        cell.Row = row;
                        cell.DataContext = Controller.GetProperty(row, Columns[column].FieldName);
                        cellNeedsMeasure = true;
                    }
                    if (!cell.IsVisible)
                    {
                        cell.IsVisible = true;
                        cellNeedsMeasure = true;
                    }

                    if (cellNeedsMeasure)
                    {
                        cell.InvalidateMeasure();
                        cell.Measure(Size.Infinity);
                    }

                    ColumnWidths[cell.Column.Index] = Math.Max(ColumnWidths[cell.Column.Index], cell.DesiredSize.Width);
                    if (visibleRowIndex == RowHeights.Count)
                        RowHeights.Add(0.0);
                    RowHeights[visibleRowIndex] = Math.Max(RowHeights[visibleRowIndex], cell.DesiredSize.Height);
                }

                viewportRowsHeight += RowHeights[visibleRowIndex];
                //TODO first grid line is not taking into account
                viewportRowsHeight += GridLineThickness;
                if (visibleRowIndex == 0)
                {
                    double firstRowHiddenPart = Offset.Y - offsetFloor;
                    firstVisibleRowVisiblePart = 1.0 - firstRowHiddenPart;
                    viewportRowsHeight *= firstVisibleRowVisiblePart;
                    AccumulatedRowHeightsStart = -RowHeights[0] * firstRowHiddenPart;
                }
                if (viewportRowsHeight >= availableSize.Height)
                {
                    lastVisibleRowVisiblePart = (RowHeights[visibleRowIndex] - (viewportRowsHeight - availableSize.Height)) / RowHeights[visibleRowIndex];
                }

                row++;
                cellIndex = Cells.GetRowCellIndex(row);
            }

            Cells.OptimizeFreeCells(row);
            for (int i = row - firstRow; i < RowHeights.Count; i++)
                RowHeights[i] = Double.NaN;
            CalcAccumulatedColumnRowSizes();
            UpdateGridLines();
            Viewport = new Size(availableSize.Width, row - firstRow - 2 + firstVisibleRowVisiblePart + lastVisibleRowVisiblePart);
            return new Size(ColumnWidths.Sum(), viewportRowsHeight);


            void ZeroColumnRowSizes()
            {
                if (ColumnWidths.Count != Columns.Count)
                {
                    ColumnWidths.Clear();
                    ColumnWidths.AddRange(Enumerable.Repeat(0.0, Columns.Count));
                }
                for (int i = 0; i < ColumnWidths.Count; i++)
                {
                    ColumnWidths[i] = 0.0;
                }
                for (int i = 0; i < RowHeights.Count; i++)
                {
                    RowHeights[i] = 0.0;
                }
            }

            void CalcAccumulatedColumnRowSizes()
            {
                if (AccumulatedColumnWidths.Count != ColumnWidths.Count + 1)
                {
                    AccumulatedColumnWidths.Clear();
                    AccumulatedColumnWidths.AddRange(Enumerable.Repeat(0.0, ColumnWidths.Count + 1));
                }

                //TODO AccumulatedColumnHeightsStart
                for (int i = 0; i < AccumulatedColumnWidths.Count; i++)
                {
                    if (i != 0)
                        AccumulatedColumnWidths[i] = AccumulatedColumnWidths[i - 1] + ColumnWidths[i - 1] + GridLineThickness;
                    else
                        AccumulatedColumnWidths[i] = /*AccumulatedColumnHeightsStart + */ GridLineThickness;
                }

                if (AccumulatedRowHeights.Count != RowHeights.Count + 1)
                {
                    AccumulatedRowHeights.Clear();
                    AccumulatedRowHeights.AddRange(Enumerable.Repeat(0.0, RowHeights.Count + 1));
                }

                for (int i = 0; i < AccumulatedRowHeights.Count; i++)
                {
                    if (i != 0)
                    {
                        if (!Double.IsNaN(RowHeights[i - 1]))
                            AccumulatedRowHeights[i] = AccumulatedRowHeights[i - 1] + RowHeights[i - 1] + GridLineThickness;
                        else
                            AccumulatedRowHeights[i] = Double.NaN;
                    }
                    else
                    {
                        AccumulatedRowHeights[i] = AccumulatedRowHeightsStart + GridLineThickness;
                    }
                }
            }

            void UpdateGridLines()
            {
                for (int i = ColumnLines.Count; i < AccumulatedColumnWidths.Count; i++)
                {
                    Line line = new Line();
                    line.Stroke = LineBrush;
                    line.StrokeThickness = GridLineThickness;
                    ColumnLines.Add(line);
                    LogicalChildren.Add(line);
                    VisualChildren.Add(line);
                }

                int lastAccumulatedColumnWidthsIndex = AccumulatedColumnWidths.Count - 1;

                for (int i = RowLines.Count; i < AccumulatedRowHeights.Count; i++)
                {
                    Line line = new Line();
                    line.Stroke = LineBrush;
                    line.StrokeThickness = GridLineThickness;
                    RowLines.Add(line);
                    LogicalChildren.Add(line);
                    VisualChildren.Add(line);
                }

                int lastAccumulatedRowHeightsIndex = AccumulatedRowHeights.Count - 1;
                for (int i = 0; i < AccumulatedRowHeights.Count; i++)
                {
                    if (Double.IsNaN(AccumulatedRowHeights[i]))
                    {
                        lastAccumulatedRowHeightsIndex = i - 1;
                        break;
                    }
                }

                for (int i = 0; i < ColumnLines.Count; i++)
                {
                    ColumnLines[i].StartPoint = new Point(AccumulatedColumnWidths[i] - GridLineThickness, 0.0);
                    ColumnLines[i].EndPoint = new Point(AccumulatedColumnWidths[i] - GridLineThickness,
                        AccumulatedRowHeights[lastAccumulatedRowHeightsIndex]);
                }

                for (int i = 0; i < RowLines.Count; i++)
                {
                    bool isVisibleLine = !Double.IsNaN(AccumulatedRowHeights[i]);
                    RowLines[i].IsVisible = isVisibleLine;
                    if (isVisibleLine)
                    {
                        RowLines[i].StartPoint = new Point(0.0, AccumulatedRowHeights[i] - GridLineThickness);
                        RowLines[i].EndPoint = new Point(AccumulatedColumnWidths[lastAccumulatedColumnWidthsIndex],
                            AccumulatedRowHeights[i] - GridLineThickness);
                    }
                }

                foreach (var line in ColumnLines.Concat(RowLines))
                {
                    line.InvalidateMeasure();
                    line.Measure(Size.Infinity);
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsVisible)
                    cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column.Index],
                        AccumulatedRowHeights[cell.VisibleRow],
                        cell.DesiredSize.Width,
                        cell.DesiredSize.Height));
            }
            //TODO is it a right place?
            ((ILogicalScrollable)this).InvalidateScroll?.Invoke();
            return finalSize;
        }

        public void RecreateCells(object itemsSource)
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();
            
            Controller.SetItemsSource(itemsSource);
            Action<GridCell> newCellPanelAction = cell =>
            {
                LogicalChildren.Add(cell);
                VisualChildren.Add(cell);
            };
            Cells = new GridCells(Columns, newCellPanelAction);
        }


        bool ILogicalScrollable.BringIntoView(IControl target, Rect targetRect)
        {
            return false;
        }

        IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl @from)
        {
            return null;
        }

        bool ILogicalScrollable.CanHorizontallyScroll
        {
            get => false;
            set { }
        }

        bool ILogicalScrollable.CanVerticallyScroll
        {
            get => true;
            set { }
        }

        bool ILogicalScrollable.IsLogicalScrollEnabled => true;

        Action ILogicalScrollable.InvalidateScroll { get; set; }

        Size ILogicalScrollable.ScrollSize => new Size(0.0, 1.0);

        Size ILogicalScrollable.PageScrollSize => new Size(0.0, Viewport.Height - 1.0);
    }
}