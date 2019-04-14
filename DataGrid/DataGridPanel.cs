using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace Ruthenium
{
    public class DataGridPanel : Control
    {
        protected DataController Controller { get; } = new DataController();

        protected List<DataGridColumn> Columns { get; }

        protected List<DataGridCell> Cells { get; } = new List<DataGridCell>(512);
        
        protected List<double> ColumnWidths { get; } = new List<double>();

        protected List<double> AccumulatedColumnWidths { get; } = new List<double>();
        
        protected List<double> RowHeights { get; } = new List<double>(64);

        protected List<double> AccumulatedRowHeights { get; } = new List<double>(64);
        
        protected List<Line> ColumnLines { get; } = new List<Line>();
        
        protected List<Line> RowLines { get; } = new List<Line>(64);

        public DataGridPanel(List<DataGridColumn> columns)
        {
            Columns = columns;
        }

        private void ZeroColumnRowSizes()
        {
            if (ColumnWidths.Count != Columns.Count)
            {
                ColumnWidths.Clear();
                ColumnWidths.AddRange(Enumerable.Repeat(0.0, Columns.Count));
            }
            else
            {
                for (int i = 0; i < ColumnWidths.Count; i++)
                {
                    ColumnWidths[i] = 0.0;
                }
            }

            if (RowHeights.Count != Controller.Count)
            {
                RowHeights.Clear();
                RowHeights.AddRange(Enumerable.Repeat(0.0, Controller.Count));
            }
            else
            {
                for (int i = 0; i < RowHeights.Count; i++)
                {
                    RowHeights[i] = 0.0;
                }
            }
        }

        private void CalcAccumulatedColumnRowSizes()
        {
            AccumulatedColumnWidths.Clear();
            for (int i = 0; i < ColumnWidths.Count + 1; i++)
            {
                if (i == 0)
                    AccumulatedColumnWidths.Add(0.0);
                else
                    AccumulatedColumnWidths.Add(AccumulatedColumnWidths[i - 1] + ColumnWidths[i - 1]);
            }
            
            AccumulatedRowHeights.Clear();
            for (int i = 0; i < RowHeights.Count + 1; i++)
            {
                if (i == 0)
                    AccumulatedRowHeights.Add(0.0);
                else
                    AccumulatedRowHeights.Add(AccumulatedRowHeights[i - 1] + RowHeights[i - 1]);
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (Columns.Count == 0)
                return Size.Empty;

            ZeroColumnRowSizes();

            foreach (var cell in Cells)
            {
                cell.Measure(Size.Infinity);
                ColumnWidths[cell.Column] = Math.Max(ColumnWidths[cell.Column], cell.DesiredSize.Width);
                RowHeights[cell.Row] = Math.Max(RowHeights[cell.Row], cell.DesiredSize.Height);
            }

            CalcAccumulatedColumnRowSizes();

            return new Size(ColumnWidths.Sum(), RowHeights.Sum());
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (var cell in Cells)
            {
                cell.Arrange(new Rect(AccumulatedColumnWidths[cell.Column], AccumulatedRowHeights[cell.Row],
                    cell.DesiredSize.Width, cell.DesiredSize.Height));
            }

            for (int i = 0; i < ColumnLines.Count; i++)
            {
                ColumnLines[i].StartPoint = new Point(AccumulatedColumnWidths[i], 0.0);
                ColumnLines[i].EndPoint = new Point(AccumulatedColumnWidths[i],
                    AccumulatedRowHeights[AccumulatedRowHeights.Count - 1]);
            }

            for (int i = 0; i < RowLines.Count; i++)
            {
                RowLines[i].StartPoint = new Point(0.0, AccumulatedRowHeights[i]);
                RowLines[i].EndPoint = new Point(AccumulatedColumnWidths[AccumulatedColumnWidths.Count - 1],
                    AccumulatedRowHeights[i]);
            }

            return finalSize;
        }


        public void CreateContent(object itemsSource)
        {
            LogicalChildren.Clear();
            VisualChildren.Clear();
            
            Controller.SetItemsSource(itemsSource);
            for (int row = 0; row < Controller.Count; row++)
            {
                for (int column = 0; column < Columns.Count; column++)
                {
                    var cell = new DataGridCell(row, column,
                        Controller.GetPropertyText(row, Columns[column].FieldName));
                    Cells.Add(cell);
                    LogicalChildren.Add(cell);
                    VisualChildren.Add(cell);
                }
            }

            SolidColorBrush lineBrush = new SolidColorBrush();
            lineBrush.Color = Colors.Black;
            ColumnLines.Clear();
            for (int i = 0; i < Columns.Count + 1; i++)
            {
                Line line = new Line();
                line.Stroke = lineBrush;
                line.StrokeThickness = 1.0;
                ColumnLines.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

            RowLines.Clear();
            for (int i = 0; i < Controller.Count + 1; i++)
            {
                Line line = new Line();
                line.Stroke = lineBrush;
                line.StrokeThickness = 1.0;
                RowLines.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }
        }
    }
}