using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace Ruthenium.DataGrid
{
    public class GridPanel : TemplatedControl
    {
        private DataGrid GridControl { get; }

        private List<ColumnHeader> ColumnHeaders { get; } = new List<ColumnHeader>();

        private List<Line> ColumnHeaderLines { get; } = new List<Line>();

        private Line HorizontalHeaderLine { get; set; }

        private List<double> ColumnHeaderWidths { get; } = new List<double>();

        private double ColumnHeadersHeight { get; set; }

        private CellsPanel CellsPanel { get; }


        public GridPanel(DataGrid gridControl)
        {
            GridControl = gridControl;
            CellsPanel = new CellsPanel(gridControl);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            ColumnHeadersHeight = 0.0;
            foreach (var columnHeader in ColumnHeaders)
            {
                columnHeader.Measure(Size.Infinity);
                ColumnHeadersHeight = Math.Max(ColumnHeadersHeight, columnHeader.DesiredSize.Height);
            }

            CalcColumnHeaderWidths();
            MeasureColumnHeaderLines();
            CellsPanel.Measure(new Size(availableSize.Width, availableSize.Height - ColumnHeadersHeight - DataGrid.GridLineThickness));
            return new Size(ColumnHeaderWidths[ColumnHeaderWidths.Count - 1],
                ColumnHeadersHeight + DataGrid.GridLineThickness + CellsPanel.DesiredSize.Height);

            void CalcColumnHeaderWidths()
            {
                if (ColumnHeaderWidths.Count != ColumnHeaders.Count + 1)
                {
                    ColumnHeaderWidths.Clear();
                    ColumnHeaderWidths.AddRange(Enumerable.Repeat(0.0, ColumnHeaders.Count + 1));
                }
                for (int i = 0; i < ColumnHeaderWidths.Count; i++)
                {
                    if (i != 0)
                        ColumnHeaderWidths[i] = ColumnHeaderWidths[i - 1] + ColumnHeaders[i - 1].Column.Width + DataGrid.GridLineThickness;
                    else
                        ColumnHeaderWidths[i] = 0.0;
                }
            }

            void MeasureColumnHeaderLines()
            {
                for (int i = 0; i < ColumnHeaderLines.Count; i++)
                {
                    double x = ColumnHeaderWidths[i + 1] - DataGrid.GridLineThickness / 2.0;
                    ColumnHeaderLines[i].StartPoint = new Point(x, 0.0);
                    ColumnHeaderLines[i].EndPoint = new Point(x, ColumnHeadersHeight);
                }
                HorizontalHeaderLine.StartPoint = new Point(0.0, ColumnHeadersHeight + DataGrid.GridLineThickness / 2.0);
                HorizontalHeaderLine.EndPoint = new Point(availableSize.Width, ColumnHeadersHeight + DataGrid.GridLineThickness / 2.0);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < ColumnHeaderWidths.Count - 1; i++)
            {
                double width = ColumnHeaderWidths[i + 1] - ColumnHeaderWidths[i] - DataGrid.GridLineThickness;
                width = Math.Min(width, finalSize.Width - ColumnHeaderWidths[i]);
                width = Math.Max(width, 0.0);
                ColumnHeaders[i].Arrange(new Rect(ColumnHeaderWidths[i], 0.0, width, ColumnHeadersHeight));
            }
            CellsPanel.Arrange(new Rect(0.0, ColumnHeadersHeight + DataGrid.GridLineThickness,
                finalSize.Width, finalSize.Height - ColumnHeadersHeight - DataGrid.GridLineThickness));

            return finalSize;
        }

        public void RecreateContent()
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();

            ColumnHeaders.Clear();
            ColumnHeaderLines.Clear();
            for (int i = 0; i < GridControl.Columns.Count; i++)
            {
                GridControl.Columns[i].Index = i;
                var columnHeader = GridControl.Columns[i].CreateColumnHeader();
                ColumnHeaders.Add(columnHeader);
                LogicalChildren.Add(columnHeader);
                VisualChildren.Add(columnHeader);
                var line = new Line() {Stroke = DataGrid.LineBrush, StrokeThickness = DataGrid.GridLineThickness};
                ColumnHeaderLines.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

            HorizontalHeaderLine = new Line() {Stroke = DataGrid.LineBrush, StrokeThickness = DataGrid.GridLineThickness};
            LogicalChildren.Add(HorizontalHeaderLine);
            VisualChildren.Add(HorizontalHeaderLine);

            CellsPanel.RecreateCells();
            LogicalChildren.Add(CellsPanel);
            VisualChildren.Add(CellsPanel);
        }

        public void UpdateSelection()
        {
            CellsPanel.UpdateSelection();
        }

        public void FocusRow(int row)
        {
            CellsPanel.FocusRow(row);
        }

        public void ScrollLineUp()
        {
            CellsPanel.ScrollLineUp();
        }

        public void ScrollLineDown()
        {
            CellsPanel.ScrollLineDown();
        }
    }
}
