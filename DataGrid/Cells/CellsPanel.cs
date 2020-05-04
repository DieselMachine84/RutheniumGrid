using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;

namespace Ruthenium.DataGrid
{
    public class CellsPanel : TemplatedControl
    {
        private DataGrid GridControl { get; }

        private CellsCollection Cells { get; set; }

        private ScrollBar ScrollBar { get; } = new ScrollBar
        {
            Orientation = Orientation.Vertical,
            Visibility = ScrollBarVisibility.Visible
        };

        private double _scrollOffset;

        private double ScrollOffset
        {
            get => _scrollOffset;
            set
            {
                _scrollOffset = value;
                _scrollOffset = Math.Max(_scrollOffset, 0.0);
                _scrollOffset = Math.Min(_scrollOffset, GridControl.Controller.Count - 0.001);
            }
        }

        private List<double> AccumulatedColumnWidths { get; } = new List<double>();

        private List<double> RowHeights { get; } = new List<double>();

        private List<double> AccumulatedRowHeights { get; } = new List<double>();

        private double AccumulatedRowHeightsStart { get; set; }

        private List<Line> ColumnLines { get; } = new List<Line>();

        private List<Line> RowLines { get; } = new List<Line>();


        public CellsPanel(DataGrid gridControl)
        {
            GridControl = gridControl;
            ScrollBar.Scroll += ScrollBarOnScroll;
        }

        private void ScrollBarOnScroll(object sender, ScrollEventArgs e)
        {
            ScrollOffset = e.NewValue;
            InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (GridControl.Columns.Count == 0)
                return Size.Empty;

            for (int i = 0; i < RowHeights.Count; i++)
                RowHeights[i] = 0.0;

            double offsetFloor = Math.Floor(ScrollOffset);
            int firstRow = Convert.ToInt32(offsetFloor);
            int row = firstRow;
            int cellIndex = Cells.GetFirstRowCellIndex(row);
            double viewportRowsHeight = 0.0;
            double firstVisibleRowVisiblePart = 1.0;
            double lastVisibleRowVisiblePart = 1.0;

            //TODO zero, very big one and two rows
            while (viewportRowsHeight < availableSize.Height && row < GridControl.Controller.Count)
            {
                int visibleRowIndex = row - firstRow;

                for (int column = 0; column < GridControl.Columns.Count; column++)
                {
                    bool cellNeedsMeasure = false;
                    var cell = Cells[cellIndex + column];
                    cell.VisibleRow = visibleRowIndex;
                    if (cell.Row != row)
                    {
                        cell.Row = row;
                        cell.DataContext = GridControl.Controller.GetProperty(row, GridControl.Columns[column].FieldName);
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

                    if (visibleRowIndex == RowHeights.Count)
                        RowHeights.Add(0.0);
                    RowHeights[visibleRowIndex] = Math.Max(RowHeights[visibleRowIndex], cell.DesiredSize.Height);
                }

                viewportRowsHeight += RowHeights[visibleRowIndex];
                if (visibleRowIndex == 0)
                {
                    double firstRowHiddenPart = ScrollOffset - offsetFloor;
                    firstVisibleRowVisiblePart = 1.0 - firstRowHiddenPart;
                    viewportRowsHeight *= firstVisibleRowVisiblePart;
                    AccumulatedRowHeightsStart = -RowHeights[0] * firstRowHiddenPart;
                }
                if (viewportRowsHeight >= availableSize.Height)
                {
                    lastVisibleRowVisiblePart = (RowHeights[visibleRowIndex] - (viewportRowsHeight - availableSize.Height)) / RowHeights[visibleRowIndex];
                }
                viewportRowsHeight += DataGrid.GridLineThickness;

                row++;
                cellIndex = Cells.GetRowCellIndex(row);
            }

            Cells.OptimizeFreeCells(row);
            for (int i = row - firstRow; i < RowHeights.Count; i++)
                RowHeights[i] = Double.NaN;
            CalcAccumulatedColumnRowSizes();
            UpdateGridLines();
            UpdateScrollBar();
            return new Size(AccumulatedColumnWidths.Last(), viewportRowsHeight);


            void CalcAccumulatedColumnRowSizes()
            {
                var columns = GridControl.Columns;
                if (AccumulatedColumnWidths.Count != columns.Count + 1)
                {
                    AccumulatedColumnWidths.Clear();
                    AccumulatedColumnWidths.AddRange(Enumerable.Repeat(0.0, columns.Count + 1));
                }

                for (int i = 0; i < AccumulatedColumnWidths.Count; i++)
                {
                    if (i != 0)
                        AccumulatedColumnWidths[i] = AccumulatedColumnWidths[i - 1] + columns[i - 1].Width + DataGrid.GridLineThickness;
                    else
                        AccumulatedColumnWidths[i] = 0.0;
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
                            AccumulatedRowHeights[i] = AccumulatedRowHeights[i - 1] + RowHeights[i - 1] + DataGrid.GridLineThickness;
                        else
                            AccumulatedRowHeights[i] = Double.NaN;
                    }
                    else
                    {
                        AccumulatedRowHeights[i] = AccumulatedRowHeightsStart;
                        if (AccumulatedRowHeightsStart < -Single.Epsilon)
                            AccumulatedRowHeights[i] += DataGrid.GridLineThickness;
                    }
                }
            }

            void UpdateGridLines()
            {
                for (int i = ColumnLines.Count; i < AccumulatedColumnWidths.Count - 1; i++)
                {
                    Line line = new Line();
                    line.Stroke = DataGrid.LineBrush;
                    line.StrokeThickness = DataGrid.GridLineThickness;
                    ColumnLines.Add(line);
                    LogicalChildren.Add(line);
                    VisualChildren.Add(line);
                }

                int lastAccumulatedColumnWidthsIndex = AccumulatedColumnWidths.Count - 1;

                for (int i = RowLines.Count; i < AccumulatedRowHeights.Count - 1; i++)
                {
                    Line line = new Line();
                    line.Stroke = DataGrid.LineBrush;
                    line.StrokeThickness = DataGrid.GridLineThickness;
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
                    ColumnLines[i].StartPoint = new Point(AccumulatedColumnWidths[i + 1] - DataGrid.GridLineThickness / 2.0, 0.0);
                    ColumnLines[i].EndPoint = new Point(AccumulatedColumnWidths[i + 1] - DataGrid.GridLineThickness / 2.0,
                        AccumulatedRowHeights[lastAccumulatedRowHeightsIndex]);
                }

                for (int i = 0; i < RowLines.Count; i++)
                {
                    bool isVisibleLine = !Double.IsNaN(AccumulatedRowHeights[i + 1]);
                    RowLines[i].IsVisible = isVisibleLine;
                    if (isVisibleLine)
                    {
                        RowLines[i].StartPoint = new Point(0.0, AccumulatedRowHeights[i + 1] - DataGrid.GridLineThickness / 2.0);
                        RowLines[i].EndPoint = new Point(AccumulatedColumnWidths[lastAccumulatedColumnWidthsIndex],
                            AccumulatedRowHeights[i + 1] - DataGrid.GridLineThickness / 2.0);
                    }
                }

                foreach (var line in ColumnLines.Concat(RowLines))
                {
                    line.InvalidateMeasure();
                    line.Measure(Size.Infinity);
                }
            }

            void UpdateScrollBar()
            {
                ScrollBar.Height = availableSize.Height;
                ScrollBar.ViewportSize = row - firstRow - 2 + firstVisibleRowVisiblePart + lastVisibleRowVisiblePart;
                ScrollBar.InvalidateMeasure();
                ScrollBar.Measure(Size.Infinity);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var cell in Cells)
            {
                if (cell.IsVisible)
                {
                    double width = AccumulatedColumnWidths[cell.Column.Index + 1] -
                                   AccumulatedColumnWidths[cell.Column.Index] - DataGrid.GridLineThickness;
                    cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column.Index],
                        AccumulatedRowHeights[cell.VisibleRow],
                        width, cell.DesiredSize.Height));
                }
            }
            ScrollBar.Arrange(new Rect(finalSize.Width - ScrollBar.Width, 0.0, ScrollBar.Width, finalSize.Height));
            return finalSize;
        }

        public void RecreateCells()
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();
            
            Action<Cell> newCellPanelAction = cell =>
            {
                LogicalChildren.Add(cell);
                VisualChildren.Add(cell);
            };
            Cells = new CellsCollection(GridControl.Columns, newCellPanelAction);

            LogicalChildren.Add(ScrollBar);
            VisualChildren.Add(ScrollBar);
            ScrollBar.Maximum = GridControl.Controller.Count;
        }
    }
}