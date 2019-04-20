using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;

namespace Ruthenium
{
    public class DataGridPanel : Control, ILogicalScrollable
    {
        private Vector _offset = new Vector(0.0, 0.0);
        
        private DataController Controller { get; } = new DataController();

        internal List<DataGridColumn> Columns { get; } = new List<DataGridColumn>();

        private DataGridCells Cells { get; set; }


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


        public DataGridPanel()
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
                    var cell = Cells[cellIndex + column];
                    cell.IsVisible = true;
                    cell.VisibleRow = visibleRowIndex;
                    if (cell.Row != row)
                    {
                        cell.Row = row;
                        cell.Text = Controller.GetPropertyText(row, Columns[column].FieldName);
                        cell.Measure(Size.Infinity);
                    }

                    ColumnWidths[cell.Column] = Math.Max(ColumnWidths[cell.Column], cell.DesiredSize.Width);
                    if (visibleRowIndex == RowHeights.Count)
                        RowHeights.Add(0.0);
                    RowHeights[visibleRowIndex] = Math.Max(RowHeights[visibleRowIndex], cell.DesiredSize.Height);
                }

                viewportRowsHeight += RowHeights[visibleRowIndex];
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

                //TODO first grid line is not taking into account
                viewportRowsHeight += 1.0; //horizontal grid line

                row++;
                cellIndex = Cells.GetRowCellIndex(row);
            }

            for (int i = row - firstRow; i < RowHeights.Count; i++)
                RowHeights[i] = Double.NaN;
            CalcAccumulatedColumnRowSizes();
            UpdateGridLines();
            Cells.OptimizeFreeCells(row);
            Viewport = new Size(availableSize.Width, row - firstRow - 2 + firstVisibleRowVisiblePart + lastVisibleRowVisiblePart);
            ((ILogicalScrollable)this).InvalidateScroll?.Invoke();
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
                        AccumulatedColumnWidths[i] = AccumulatedColumnWidths[i - 1] + ColumnWidths[i - 1] + 1.0;
                    else
                        AccumulatedColumnWidths[i] = /*AccumulatedColumnHeightsStart + */ 1.0;
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
                            AccumulatedRowHeights[i] = AccumulatedRowHeights[i - 1] + RowHeights[i - 1] + 1.0;
                        else
                            AccumulatedRowHeights[i] = Double.NaN;
                    }
                    else
                    {
                        AccumulatedRowHeights[i] = AccumulatedRowHeightsStart + 1.0;
                    }
                }
            }

            void UpdateGridLines()
            {
                if (ColumnLines.Count != AccumulatedColumnWidths.Count)
                {
                    foreach (var columnLine in ColumnLines)
                    {
                        LogicalChildren.Remove(columnLine);
                        VisualChildren.Remove(columnLine);
                    }
                    ColumnLines.Clear();
                    for (int i = 0; i < AccumulatedColumnWidths.Count; i++)
                    {
                        Line line = new Line();
                        line.Stroke = LineBrush;
                        line.StrokeThickness = 1.0;
                        ColumnLines.Add(line);
                        LogicalChildren.Add(line);
                        VisualChildren.Add(line);
                    }
                }

                int lastAccumulatedColumnWidthsIndex = AccumulatedColumnWidths.Count - 1;

                if (RowLines.Count != AccumulatedRowHeights.Count)
                {
                    foreach (var rowLine in RowLines)
                    {
                        LogicalChildren.Remove(rowLine);
                        VisualChildren.Remove(rowLine);
                    }
                    RowLines.Clear();
                    for (int i = 0; i < AccumulatedRowHeights.Count; i++)
                    {
                        Line line = new Line();
                        line.Stroke = LineBrush;
                        line.StrokeThickness = 1.0;
                        RowLines.Add(line);
                        LogicalChildren.Add(line);
                        VisualChildren.Add(line);
                    }
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
                    ColumnLines[i].StartPoint = new Point(AccumulatedColumnWidths[i] - 1.0, 0.0);
                    ColumnLines[i].EndPoint = new Point(AccumulatedColumnWidths[i] - 1.0,
                        AccumulatedRowHeights[lastAccumulatedRowHeightsIndex]);
                }

                for (int i = 0; i < RowLines.Count; i++)
                {
                    RowLines[i].StartPoint = new Point(0.0, AccumulatedRowHeights[i] - 1.0);
                    RowLines[i].EndPoint = new Point(AccumulatedColumnWidths[lastAccumulatedColumnWidthsIndex],
                        AccumulatedRowHeights[i] - 1.0);
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsVisible)
                    cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column],
                        AccumulatedRowHeights[cell.VisibleRow],
                        cell.DesiredSize.Width,
                        cell.DesiredSize.Height));
            }

            return finalSize;
        }

        public void RecreateCells(object itemsSource)
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();
            
            Controller.SetItemsSource(itemsSource);
            Action<DataGridCell> newCellPanelAction = cell =>
            {
                LogicalChildren.Add(cell);
                VisualChildren.Add(cell);
            };
            Cells = new DataGridCells(Columns.Count, newCellPanelAction);
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