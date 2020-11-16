using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Xunit;
using Xunit.Sdk;

namespace Ruthenium.DataGrid.UnitTests
{
    [Collection("DataGrid collection")]
    public class NavigationTests
    {
        private static object SyncRoot = new object();
        private MainWindow Window { get; }
        public NavigationTests(DataGridFixture fixture)
        {
            while (Window == null)
            {
                lock (SyncRoot)
                {
                    Window = (MainWindow) fixture.ApplicationLifetime?.MainWindow;
                    Thread.Sleep(100);
                }
            }
        }

        private void ShowGridWindow(Action<Window> resize, Queue<Action<DataGrid>> actions)
        {
            DataGrid CreateDataGrid()
            {
                DataGrid dataGrid = new DataGrid();
                TextColumn idColumn = new TextColumn() {FieldName = "Id", Width = 50};
                TextColumn textColumn = new TextColumn() {FieldName = "Text", Width = 100};
                dataGrid.Columns.Add(idColumn);
                dataGrid.Columns.Add(textColumn);
                dataGrid.ItemsSource = DataSources.GetSimpleDataSource(1000);
                return dataGrid;
            }

            Window.Finished = false;
            Dispatcher.UIThread.Post(() => Window.DoActions(CreateDataGrid, resize, actions));
            while (!Window.Finished)
            {
                Thread.Sleep(100);
            }

            if (Window.Exception != null)
                throw Window.Exception;
        }

        private List<Cell> GetVisibleCells(DataGrid dataGrid)
        {
            return dataGrid.GetLogicalDescendants().OfType<Cell>().Where(c => c.IsVisible)
                .OrderBy(c => c.Bounds.Y).ThenBy(c => c.Bounds.X).ToList();
        }

        private List<Cell> GetCellsByRow(DataGrid dataGrid, int row)
        {
            return dataGrid.GetLogicalDescendants().OfType<Cell>().Where(c => c.Row == row)
                .OrderBy(c => c.Bounds.X).ToList();
        }

        private string GetCellText(Cell cell)
        {
            return cell.GetLogicalDescendants().OfType<TextBlock>().First().Text;
        }

        private void CheckCellText(DataGrid dataGrid, List<ValueTuple<int, string>> expected)
        {
            var cells = GetVisibleCells(dataGrid);
            foreach (var item in expected)
            {
                Assert.Equal(item.Item2, GetCellText(cells[item.Item1]));
            }
        }

        private double GetScrollOffset(DataGrid dataGrid)
        {
            var cellsPanel = dataGrid.GetLogicalDescendants().OfType<CellsPanel>().First();
            return cellsPanel.GetLogicalDescendants().OfType<ScrollBar>().First().Value;
        }

        [Fact]
        public void ScrollDownAndUp()
        {
            //36.5 cells by 16 pixels with 1 pixel border
            //header cell 16 pixels with 1 pixel border
            //2 borders by 1 pixel for grid
            Action<Window> resize = (window) => { window.Height = (16 + 1) * 36 + 16 * 0.5 + 16 + 1 + 2; };
            var actions = new Queue<Action<DataGrid>>();
            actions.Enqueue(dataGrid =>
            {
                dataGrid.ItemsSource = DataSources.GetSimpleDataSource(1);
            });
            actions.Enqueue(dataGrid =>
            {
                dataGrid.ItemsSource = DataSources.GetSimpleDataSource(1000);
            });
            actions.Enqueue(dataGrid =>
            {
                Cell.MeasureCount = 0;
                var expected = new List<ValueTuple<int, string>>
                    {(0, "0"), (1, "A"), (72, "36"), (73, "AK")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(4, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "1"), (1, "B"), (72, "37"), (73, "AL")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(8, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "2"), (1, "C"), (72, "38"), (73, "AM")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(12, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "3"), (1, "D"), (72, "39"), (73, "AN")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(14, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "4"), (1, "E"), (72, "40"), (73, "AO")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(16, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "5"), (1, "F"), (72, "41"), (73, "AP")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(18, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "6"), (1, "G"), (72, "42"), (73, "AQ")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(20, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "7"), (1, "H"), (72, "43"), (73, "AR")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(22, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "8"), (1, "I"), (72, "44"), (73, "AS")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineUp();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(24, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "7"), (1, "H"), (72, "43"), (73, "AR")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineUp();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(26, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "6"), (1, "G"), (72, "42"), (73, "AQ")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineUp();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(28, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "5"), (1, "F"), (72, "41"), (73, "AP")};
                CheckCellText(dataGrid, expected);
                dataGrid.ScrollLineUp();
            });
            actions.Enqueue(dataGrid =>
            {
                Assert.Equal(30, Cell.MeasureCount);
                var expected = new List<ValueTuple<int, string>>
                    {(0, "4"), (1, "E"), (72, "40"), (73, "AO")};
                CheckCellText(dataGrid, expected);
            });
            ShowGridWindow(resize, actions);
        }

        [Fact]
        public void SelectionTest()
        {
            //4 cells by 16 pixels with 1 pixel border
            //1 cell by 8 pixels
            //header cell 16 pixels with 1 pixel border
            //2 borders by 1 pixel for grid
            Action<Window> resize = (window) => { window.Height = (16 + 1) * 4 + 16 * 0.5 + 16 + 1 + 2; };
            var actions = new Queue<Action<DataGrid>>();
            actions.Enqueue(dataGrid =>
            {
                for (int i = 0; i < 200; i++)
                {
                    dataGrid.ScrollLineDown();
                }
            });
            actions.Enqueue(dataGrid =>
            {
                Cell.MeasureCount = 0;
                var expected = new List<ValueTuple<int, string>>
                    {(0, "200"), (1, "GS"), (8, "204"), (9, "GW")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 200)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 201)[0].Background);
                dataGrid.SelectRow(201);
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "200"), (1, "GS"), (8, "204"), (9, "GW")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 200)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 201)[0].Background);
                dataGrid.FocusPreviousRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "200"), (1, "GS"), (8, "204"), (9, "GW")};
                CheckCellText(dataGrid, expected);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 200)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 201)[0].Background);
                dataGrid.FocusPreviousRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "199"), (1, "GR"), (8, "203"), (9, "GV")};
                CheckCellText(dataGrid, expected);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 199)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 200)[0].Background);
                dataGrid.SelectRow(202);
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "199"), (1, "GR"), (8, "203"), (9, "GV")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 199)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 202)[0].Background);
                Assert.Equal(199, GetScrollOffset(dataGrid));
                dataGrid.FocusNextRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "199"), (1, "GR"), (8, "203"), (9, "GV")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 202)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 203)[0].Background);
                Assert.Equal(199 + (9.0 / 16.0), GetScrollOffset(dataGrid));
                for (int i = 0; i < 10; i++)
                {
                    dataGrid.ScrollLineDown();
                }
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "209"), (1, "HB"), (8, "213"), (9, "HF")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 209)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 210)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 213)[0].Background);
                Assert.Equal(209 + (9.0 / 16.0), GetScrollOffset(dataGrid));
                dataGrid.FocusNextRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "204"), (1, "GW"), (8, "208"), (9, "HA")};
                CheckCellText(dataGrid, expected);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 204)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 205)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 208)[0].Background);
                Assert.Equal(204, GetScrollOffset(dataGrid));
                dataGrid.ScrollLineDown();
                dataGrid.ScrollLineDown();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "206"), (1, "GY"), (8, "210"), (9, "HC")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 206)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 207)[0].Background);
                Assert.Equal(206, GetScrollOffset(dataGrid));
                dataGrid.FocusPreviousRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "203"), (1, "GV"), (8, "207"), (9, "GZ")};
                CheckCellText(dataGrid, expected);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 203)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 204)[0].Background);
                Assert.Equal(203, GetScrollOffset(dataGrid));
                for (int i = 0; i < 10; i++)
                {
                    dataGrid.ScrollLineUp();
                }
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "193"), (1, "GL"), (8, "197"), (9, "GP")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 193)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 194)[0].Background);
                Assert.Equal(193, GetScrollOffset(dataGrid));
                dataGrid.FocusPreviousRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "198"), (1, "GQ"), (8, "202"), (9, "GU")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 198)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 199)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 202)[0].Background);
                Assert.Equal(198 + (9.0 / 16.0), GetScrollOffset(dataGrid));
                dataGrid.ScrollLineUp();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "197"), (1, "GP"), (8, "201"), (9, "GT")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 197)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 198)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 201)[0].Background);
                Assert.Equal(197 + (9.0 / 16.0), GetScrollOffset(dataGrid));
                dataGrid.FocusNextRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "199"), (1, "GR"), (8, "203"), (9, "GV")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 199)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 200)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 203)[0].Background);
                Assert.Equal(199 + (9.0 / 16.0), GetScrollOffset(dataGrid));
                dataGrid.FocusFirstRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "0"), (1, "A"), (8, "4"), (9, "E")};
                CheckCellText(dataGrid, expected);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 0)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 1)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 4)[0].Background);
                Assert.Equal(0, GetScrollOffset(dataGrid));
                dataGrid.FocusLastRow();
            });
            actions.Enqueue(dataGrid =>
            {
                var expected = new List<ValueTuple<int, string>>
                    {(0, "995"), (1, "ALH"), (8, "999"), (9, "ALL")};
                CheckCellText(dataGrid, expected);
                Assert.Null(GetCellsByRow(dataGrid, 995)[0].Background);
                Assert.Null(GetCellsByRow(dataGrid, 996)[0].Background);
                Assert.Equal(DataGrid.SelectedCellBrush, GetCellsByRow(dataGrid, 999)[0].Background);
                Assert.Equal(995 + (9.0 / 16.0), GetScrollOffset(dataGrid));
            });
            ShowGridWindow(resize, actions);
        }
    }
}