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
        private List<ColumnHeader> ColumnHeaders { get; } = new List<ColumnHeader>();
        private CellsPanel CellsPanel { get; }
        private List<Line> GridBorderLines { get; } = new List<Line>();
        private List<Line> ColumnHeaderLines { get; } = new List<Line>();
        private Line HorizontalHeaderLine { get; set; }

        static GridPanel()
        {
            ClipToBoundsProperty.OverrideDefaultValue<GridPanel>(true);
        }

        public GridPanel(DataGrid gridControl)
        {
            GridControl = gridControl;
            CellsPanel = new CellsPanel(gridControl);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            double doubleBorderThickness = GridControl.ShowBorder ? GridControl.BorderThickness * 2.0 : 0.0;
            double columnHeadersHeight = 0.0;
            foreach (var columnHeader in ColumnHeaders)
            {
                columnHeader.Measure(new Size(availableSize.Width - doubleBorderThickness, Double.PositiveInfinity));
                columnHeadersHeight = Math.Max(columnHeadersHeight, columnHeader.DesiredSize.Height);
            }
            columnHeadersHeight += GridControl.HorizontalLinesThickness;

            double columnHeadersWidth = ColumnHeaders.Sum(ch => ch.Column.Width + GridControl.VerticalLinesThickness);

            //TODO check negative height
            CellsPanel.Measure(new Size(availableSize.Width - doubleBorderThickness,
                availableSize.Height - doubleBorderThickness - columnHeadersHeight));
            return new Size(columnHeadersWidth + doubleBorderThickness,
                columnHeadersHeight + CellsPanel.DesiredSize.Height + doubleBorderThickness);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (GridControl.ShowBorder)
            {
                var halfBorderLineThickness = GridControl.BorderThickness / 2.0;
                GridBorderLines[0].StartPoint = new Point(0.0, halfBorderLineThickness);
                GridBorderLines[0].EndPoint = new Point(finalSize.Width, halfBorderLineThickness);
                GridBorderLines[1].StartPoint = new Point(0.0, finalSize.Height - halfBorderLineThickness);
                GridBorderLines[1].EndPoint = new Point(finalSize.Width, finalSize.Height - halfBorderLineThickness);
                GridBorderLines[2].StartPoint = new Point(halfBorderLineThickness, 0.0);
                GridBorderLines[2].EndPoint = new Point(halfBorderLineThickness, finalSize.Height);
                GridBorderLines[3].StartPoint = new Point(finalSize.Width - halfBorderLineThickness, 0.0);
                GridBorderLines[3].EndPoint = new Point(finalSize.Width - halfBorderLineThickness, finalSize.Height);
            }
            foreach (var borderLine in GridBorderLines)
            {
                borderLine.IsVisible = GridControl.ShowBorder;
            }

            double columnHeadersHeight = ColumnHeaders.Max(ch => ch.DesiredSize.Height) + GridControl.HorizontalLinesThickness;
            double borderThickness = GridControl.ShowBorder ? GridControl.BorderThickness : 0.0;
            double horizontalLineY = borderThickness + columnHeadersHeight - GridControl.HorizontalLinesThickness / 2.0;
            HorizontalHeaderLine.StartPoint = new Point(borderThickness, horizontalLineY);
            HorizontalHeaderLine.EndPoint = new Point(finalSize.Width - borderThickness, horizontalLineY);
            double columnHeadersWidth = borderThickness;
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                var columnHeader = ColumnHeaders[i];
                columnHeader.Arrange(new Rect(columnHeadersWidth, borderThickness, columnHeader.Column.Width, columnHeadersHeight));
                columnHeadersWidth += columnHeader.Column.Width + GridControl.VerticalLinesThickness;
                double verticalLineX = columnHeadersWidth - GridControl.VerticalLinesThickness / 2.0;
                ColumnHeaderLines[i].StartPoint = new Point(verticalLineX, borderThickness);
                ColumnHeaderLines[i].EndPoint = new Point(verticalLineX, columnHeadersHeight + borderThickness);
            }

            CellsPanel.Arrange(new Rect(borderThickness, borderThickness + columnHeadersHeight,
                finalSize.Width - 2.0 * borderThickness, finalSize.Height - 2.0 * borderThickness - columnHeadersHeight));

            return finalSize;
        }

        public void RecreateContent()
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();

            GridBorderLines.Clear();
            for (int i = 0; i < 4; i++)
            {
                var line = new Line() {Stroke = GridControl.BorderBrush, StrokeThickness = GridControl.BorderThickness};
                GridBorderLines.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

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

        /*public void UpdateSelection()
        {
            CellsPanel.UpdateSelection();
        }

        public void FocusRow(int row)
        {
            CellsPanel.FocusRow(row);
        }*/

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
