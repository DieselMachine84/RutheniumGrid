using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
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

        private int _bottomRowToFocus = -1;

        private int FirstWholeVisibleRow { get; set; }
        private int LastWholeVisibleRow { get; set; }

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
            InvalidateMeasure();
        }

        private void CellOnPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left)
            {
                Cell cell = (Cell)sender;
                GridControl.UpdateSelection(cell);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (GridControl.Columns.Count == 0)
                return Size.Empty;

            start:
            bool measureFromTop = (_bottomRowToFocus < 0);
            if (measureFromTop)
            {
                for (int i = 0; i < RowHeights.Count; i++)
                    RowHeights[i] = 0.0;
            }
            else
            {
                RowHeights.Clear();
            }

            double offsetFloor = Math.Floor(ScrollBar.Value);
            int firstRow = Convert.ToInt32(offsetFloor);
            int row = firstRow - 1;
            if (!measureFromTop)
            {
                firstRow = _bottomRowToFocus;
                row = firstRow + 1;
                _bottomRowToFocus = -1;
            }
            Cells.SetInitialRow(firstRow);
            double viewportRowsHeight = measureFromTop ? 0.0 : GridControl.HorizontalLinesThickness;
            double firstVisibleRowVisiblePart = 1.0;
            double lastVisibleRowVisiblePart = 1.0;
            FirstWholeVisibleRow = LastWholeVisibleRow = -1;

            //TODO zero, very big one and two rows
            while (viewportRowsHeight < availableSize.Height)
            {
                if (measureFromTop)
                {
                    if (row == GridControl.Controller.Count - 1)
                        break;
                    row++;
                }
                else
                {
                    if (row == 0)
                        break;
                    row--;
                }

                int visibleRowIndex = Math.Abs(row - firstRow);
                for (int column = 0; column < GridControl.Columns.Count; column++)
                {
                    bool cellNeedsMeasure = false;
                    var cell = Cells.GetCell(row - firstRow, column);
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
                        UpdateCellSelection(cell);
                        cell.InvalidateMeasure();
                        cell.Measure(Size.Infinity);
                    }

                    if (visibleRowIndex == RowHeights.Count)
                        RowHeights.Add(0.0);
                    RowHeights[visibleRowIndex] = Math.Max(RowHeights[visibleRowIndex], cell.DesiredSize.Height);
                }

                viewportRowsHeight += RowHeights[visibleRowIndex];
                if (measureFromTop && visibleRowIndex == 0)
                {
                    double firstRowHiddenPart = ScrollBar.Value - offsetFloor;
                    firstVisibleRowVisiblePart = 1.0 - firstRowHiddenPart;
                    viewportRowsHeight *= firstVisibleRowVisiblePart;
                    AccumulatedRowHeightsStart = -RowHeights[0] * firstRowHiddenPart;
                }

                if (viewportRowsHeight >= availableSize.Height)
                {
                    double visibleRowVisiblePart = 
                        (RowHeights[visibleRowIndex] - (viewportRowsHeight - availableSize.Height)) /
                        RowHeights[visibleRowIndex];
                    if (measureFromTop)
                    {
                        lastVisibleRowVisiblePart = visibleRowVisiblePart;
                    }
                    else
                    {
                        firstVisibleRowVisiblePart = visibleRowVisiblePart;
                        double firstRowHiddenPart = 1.0 - firstVisibleRowVisiblePart;
                        AccumulatedRowHeightsStart = -RowHeights[RowHeights.Count - 1] * firstRowHiddenPart;
                    }
                }

                viewportRowsHeight += GridControl.HorizontalLinesThickness;
            }

            if (row < 0)
            {
                ScrollBar.Value = 0.0;
                goto start;
            }

            if (!measureFromTop)
            {
                //TODO top grid line is not drawn if it should and we have one pixel of next raw in the bottom
                int temp = firstRow;
                firstRow = row;
                row = temp;
                Cells.SetInitialRow(firstRow);
                RowHeights.Reverse();
                ScrollBar.Value = firstRow + (1.0 - firstVisibleRowVisiblePart);
            }

            FirstWholeVisibleRow = firstRow;
            if (firstVisibleRowVisiblePart < 1.0)
                FirstWholeVisibleRow++;
            LastWholeVisibleRow = row;
            if (lastVisibleRowVisiblePart < 1.0)
                LastWholeVisibleRow--;
            if (FirstWholeVisibleRow > LastWholeVisibleRow)
            {
                //TODO check that all works with this
                FirstWholeVisibleRow = LastWholeVisibleRow = -1;
            }

            //TODO fix bug when row == GridControl.Controller.Count
            Cells.OptimizeFreeCells(row + 1);
            Cells.UpdateVisibility(row + 1);
            for (int i = row - firstRow + 1; i < RowHeights.Count; i++)
                RowHeights[i] = Double.NaN;
            CalcAccumulatedColumnRowSizes();
            UpdateGridLines();
            UpdateScrollBar();
            return new Size(AccumulatedColumnWidths[AccumulatedColumnWidths.Count - 1], viewportRowsHeight);


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
                        AccumulatedColumnWidths[i] = AccumulatedColumnWidths[i - 1] + columns[i - 1].Width + GridControl.VerticalLinesThickness;
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
                            AccumulatedRowHeights[i] = AccumulatedRowHeights[i - 1] + RowHeights[i - 1] + GridControl.HorizontalLinesThickness;
                        else
                            AccumulatedRowHeights[i] = Double.NaN;
                    }
                    else
                    {
                        AccumulatedRowHeights[i] = AccumulatedRowHeightsStart;
                        if (AccumulatedRowHeightsStart < -Single.Epsilon)
                            AccumulatedRowHeights[i] += GridControl.HorizontalLinesThickness;
                    }
                }
            }

            //TODO lines are added many times?
            void UpdateGridLines()
            {
                for (int i = ColumnLines.Count; i < AccumulatedColumnWidths.Count - 1; i++)
                {
                    Line line = new Line();
                    line.Stroke = GridControl.VerticalLinesBrush;
                    line.StrokeThickness = GridControl.VerticalLinesThickness;
                    ColumnLines.Add(line);
                    LogicalChildren.Add(line);
                    VisualChildren.Add(line);
                }

                int lastAccumulatedColumnWidthsIndex = AccumulatedColumnWidths.Count - 1;

                for (int i = RowLines.Count; i < AccumulatedRowHeights.Count - 1; i++)
                {
                    Line line = new Line();
                    line.Stroke = GridControl.HorizontalLinesBrush;
                    line.StrokeThickness = GridControl.HorizontalLinesThickness;
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
                    ColumnLines[i].StartPoint = new Point(AccumulatedColumnWidths[i + 1] - GridControl.VerticalLinesThickness / 2.0, 0.0);
                    ColumnLines[i].EndPoint = new Point(AccumulatedColumnWidths[i + 1] - GridControl.VerticalLinesThickness / 2.0,
                        AccumulatedRowHeights[lastAccumulatedRowHeightsIndex]);
                }

                for (int i = 0; i < RowLines.Count; i++)
                {
                    bool isVisibleLine = (i + 1 < AccumulatedRowHeights.Count) && !Double.IsNaN(AccumulatedRowHeights[i + 1]);
                    RowLines[i].IsVisible = isVisibleLine;
                    if (isVisibleLine)
                    {
                        RowLines[i].StartPoint = new Point(0.0, AccumulatedRowHeights[i + 1] - GridControl.HorizontalLinesThickness / 2.0);
                        RowLines[i].EndPoint = new Point(AccumulatedColumnWidths[lastAccumulatedColumnWidthsIndex],
                            AccumulatedRowHeights[i + 1] - GridControl.HorizontalLinesThickness / 2.0);
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
                //TODO: grid should not scroll entirely
                ScrollBar.Height = availableSize.Height;
                ScrollBar.ViewportSize = row - firstRow - 2 + firstVisibleRowVisiblePart + lastVisibleRowVisiblePart;
                ScrollBar.InvalidateMeasure();
                ScrollBar.Measure(Size.Infinity);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int firstRow = Cells.GetInitialRow();
            foreach (var cell in Cells.GetVisibleCells())
            {
                double width = AccumulatedColumnWidths[cell.Column.Index + 1] -
                               AccumulatedColumnWidths[cell.Column.Index] - GridControl.VerticalLinesThickness;
                cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column.Index],
                    AccumulatedRowHeights[cell.Row - firstRow],
                    width, cell.DesiredSize.Height));
            }

            ScrollBar.Arrange(new Rect(finalSize.Width - ScrollBar.Width, 0.0, ScrollBar.Width, finalSize.Height));
            return finalSize;
        }

        public void RecreateCells()
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();

            Cells = new CellsCollection(GridControl.Columns, AddCellAction, RemoveCellAction);

            LogicalChildren.Add(ScrollBar);
            VisualChildren.Add(ScrollBar);
            //TODO recalculate with viewport
            ScrollBar.Maximum = GridControl.Controller.Count;

            void AddCellAction(Cell cell)
            {
                cell.PointerReleased += CellOnPointerReleased;
                LogicalChildren.Add(cell);
                VisualChildren.Add(cell);
            }

            void RemoveCellAction(Cell cell)
            {
                cell.PointerReleased -= CellOnPointerReleased;
                LogicalChildren.Remove(cell);
                VisualChildren.Remove(cell);
            }
        }

        private void UpdateCellSelection(Cell cell)
        {
            cell.Background = GridControl.IsSelected(cell.Row) ? DataGrid.SelectedCellBrush : GridControl.DataAreaBackground;
        }
        public void UpdateSelection()
        {
            foreach (var cell in Cells.GetVisibleCells())
            {
                UpdateCellSelection(cell);
            }
        }

        public void FocusRow(int row)
        {
            //TODO check -1
            if (row < FirstWholeVisibleRow)
            {
                ScrollBar.Value = row;
                InvalidateMeasure();
            }

            if (row > LastWholeVisibleRow)
            {
                _bottomRowToFocus = row;
                InvalidateMeasure();
            }
        }
        
        public void ScrollLineUp()
        {
            //TODO check value is in range
            ScrollBar.Value -= 1.0;
            InvalidateMeasure();
        }

        public void ScrollLineDown()
        {
            //TODO check value is in range
            ScrollBar.Value += 1.0;
            InvalidateMeasure();
        }
    }
}
