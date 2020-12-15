using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace Ruthenium.DataGrid
{
    public class GridPanel : Control
    {
        private DataGrid GridControl { get; }
        private CellsPanel CellsPanel { get; }
        private List<ColumnHeader> ColumnHeaders { get; } = new List<ColumnHeader>();
        private List<Line> ColumnHeaderLines { get; } = new List<Line>();
        private Line HorizontalHeaderLine { get; set; }
        private double ColumnHeadersHeight { get; set; }

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

            double columnHeadersWidth = 0.0;
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                var columnHeader = ColumnHeaders[i];
                columnHeadersWidth += columnHeader.Column.Width;
                double lineX = columnHeadersWidth + GridControl.VerticalLinesThickness / 2.0;
                ColumnHeaderLines[i].StartPoint = new Point(lineX, 0.0);
                ColumnHeaderLines[i].EndPoint = new Point(lineX, ColumnHeadersHeight);
                columnHeadersWidth += GridControl.VerticalLinesThickness;
            }

            var columnHeaderWithHalfLineHeight = ColumnHeadersHeight + GridControl.HorizontalLinesThickness / 2.0;
            var columnHeaderWithLineHeight = ColumnHeadersHeight + GridControl.HorizontalLinesThickness;
            HorizontalHeaderLine.StartPoint = new Point(0.0, columnHeaderWithHalfLineHeight);
            HorizontalHeaderLine.EndPoint = new Point(columnHeadersWidth, columnHeaderWithHalfLineHeight);
            CellsPanel.Measure(new Size(availableSize.Width, availableSize.Height - columnHeaderWithLineHeight));
            return new Size(columnHeadersWidth, columnHeaderWithLineHeight + CellsPanel.DesiredSize.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double columnHeadersWidth = 0.0;
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                var columnHeader = ColumnHeaders[i];
                ColumnHeaders[i].Arrange(new Rect(columnHeadersWidth, 0.0, columnHeader.Column.Width, ColumnHeadersHeight));
                columnHeadersWidth += columnHeader.Column.Width + GridControl.VerticalLinesThickness;
            }

            var columnHeaderWithLineHeight = ColumnHeadersHeight + GridControl.HorizontalLinesThickness;
            CellsPanel.Arrange(new Rect(0.0, columnHeaderWithLineHeight,
                finalSize.Width, finalSize.Height - columnHeaderWithLineHeight));

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
                var line = new Line() {Stroke = GridControl.VerticalLinesBrush, StrokeThickness = GridControl.VerticalLinesThickness};
                ColumnHeaderLines.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

            HorizontalHeaderLine = new Line() {Stroke = GridControl.HorizontalLinesBrush, StrokeThickness = GridControl.HorizontalLinesThickness};
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
