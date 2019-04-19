using System;
using System.Collections.Generic;
using System.Drawing.Text;
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
        private DataController Controller { get; } = new DataController();

        internal List<DataGridColumn> Columns { get; } = new List<DataGridColumn>();

        private DataGridCells Cells { get; set; }


        private List<double> ColumnWidths { get; } = new List<double>();

        private List<double> AccumulatedColumnWidths { get; } = new List<double>();

        private List<double> RowHeights { get; } = new List<double>();

        private List<double> AccumulatedRowHeights { get; } = new List<double>();

        private SolidColorBrush LineBrush { get; }


        private List<Line> ColumnLines { get; } = new List<Line>();

        private List<Line> RowLines { get; } = new List<Line>();


        public Size Extent => new Size(DesiredSize.Width, Controller.Count);

        public Size Viewport { get; private set; } = Size.Empty;

        public Vector Offset { get; set; } = new Vector(0.0, 0.0);


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

            double viewportRowsHeight = 0.0;
            int viewportRowsCount = 0;
            int firstRow = Convert.ToInt32(Math.Floor(Offset.Y));
            int row = firstRow;
            int cellIndex = Cells.GetFirstRowCellIndex(row);

            while (viewportRowsHeight < availableSize.Height && row < Controller.Count)
            {
                for (int column = 0; column < Columns.Count; column++)
                {
                    var cell = Cells[cellIndex + column];
                    cell.IsVisible = true;
                    if (cell.Row != row)
                    {
                        cell.Row = row;
                        cell.Text = Controller.GetPropertyText(row, Columns[column].FieldName);
                        cell.Measure(Size.Infinity);
                    }

                    ColumnWidths[cell.Column] = Math.Max(ColumnWidths[cell.Column], cell.DesiredSize.Width);
                    int rowHeightsIndex = row - firstRow;
                    if (rowHeightsIndex == RowHeights.Count)
                    {
                        RowHeights.Add(0.0);
                    }
                    RowHeights[rowHeightsIndex] = Math.Max(RowHeights[rowHeightsIndex], cell.DesiredSize.Height);
                }

                viewportRowsHeight += RowHeights[row];
                viewportRowsCount++;
                row++;
                cellIndex = Cells.GetRowCellIndex(row);
            }

            double firstRowVisibleHeight = Math.Ceiling(Offset.Y) - Offset.Y;
            if (firstRowVisibleHeight > Single.Epsilon)
                RowHeights[0] *= firstRowVisibleHeight;
            for (int i = row - firstRow; i < RowHeights.Count; i++)
                RowHeights[i] = -1.0;
            CalcAccumulatedColumnRowSizes();
            Cells.OptimizeFreeCells(row);
            Viewport = new Size(availableSize.Width, viewportRowsCount);
            return new Size(ColumnWidths.Sum(), RowHeights.Sum());


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

                for (int i = 0; i < AccumulatedColumnWidths.Count; i++)
                {
                    if (i != 0)
                        AccumulatedColumnWidths[i] = AccumulatedColumnWidths[i - 1] + ColumnWidths[i - 1] + 1.0;
                    else
                        AccumulatedColumnWidths[i] = 1.0;
                }

                if (AccumulatedRowHeights.Count != RowHeights.Count)
                {
                    AccumulatedRowHeights.Clear();
                    AccumulatedRowHeights.AddRange(Enumerable.Repeat(0.0, RowHeights.Count + 1));
                }

                for (int i = 0; i < AccumulatedRowHeights.Count; i++)
                {
                    if (i != 0)
                    {
                        if (RowHeights[i - 1] >= 0.0)
                            AccumulatedRowHeights[i] = AccumulatedRowHeights[i - 1] + RowHeights[i - 1] + 1.0;
                        else
                            AccumulatedRowHeights[i] = -1.0;
                    }
                    else
                    {
                        AccumulatedRowHeights[i] = 1.0;
                    }
                }
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsVisible)
                    cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column],
                        AccumulatedRowHeights[cell.Row],
                        cell.DesiredSize.Width,
                        cell.DesiredSize.Height));
            }

            ArrangeGridLines();

            return finalSize;

            void ArrangeGridLines()
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
                        LogicalChildren.Add(rowLine);
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
                    if (AccumulatedRowHeights[i] < 0.0)
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
            throw new NotImplementedException();
        }

        IControl ILogicalScrollable.GetControlInDirection(NavigationDirection direction, IControl @from)
        {
            throw new NotImplementedException();
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

        Size ILogicalScrollable.ScrollSize => throw new NotImplementedException();

        Size ILogicalScrollable.PageScrollSize => throw new NotImplementedException();
    }
}