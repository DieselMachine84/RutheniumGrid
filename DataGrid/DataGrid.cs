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
    public class DataGrid : TemplatedControl
    {
        internal const double GridLineThickness = 1.0;
        internal static SolidColorBrush LineBrush { get; } = new SolidColorBrush {Color = Colors.Black};
        internal static SolidColorBrush SelectedCellBrush { get; } = new SolidColorBrush {Color = Color.FromRgb(0x11, 0x9E, 0xDA)};

        private IBrush dataAreaBackground;

        public static readonly DirectProperty<DataGrid, IBrush> DataAreaBackgroundProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, IBrush>(nameof(DataAreaBackground),
                o => o.DataAreaBackground, (o, v) => o.DataAreaBackground = v);

        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource;

        private List<Line> Border { get; } = new List<Line>();

        private GridPanel Panel { get; }

        internal DataController Controller { get; } = new DataController();
        
        public IBrush DataAreaBackground
        {
            get => dataAreaBackground;
            set => SetAndRaise(DataAreaBackgroundProperty, ref dataAreaBackground, value);
        }

        public object ItemsSource
        {
            get => _itemsSource;
            set => SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
        }
        internal List<SelectedPair> SelectedRows { get; private set; } = new List<SelectedPair>();

        public SelectionType SelectionType { get; set; } = SelectionType.Single;

        public int FocusedRow
        {
            get
            {
                //TODO use focus, not selection
                return SelectedRows.Count > 0 ? SelectedRows[0].From : -1;
            }
            //TODO implement set
        }

        public List<Column> Columns { get; } = new List<Column>();

        static DataGrid()
        {
            //TODO AddClassHandler?
            ItemsSourceProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.ItemsSourceChanged(e));
            FocusableProperty.OverrideDefaultValue<DataGrid>(true);
        }

        public DataGrid()
        {
            //TODO: move to GridPanel?
            for (int i = 0; i < 4; i++)
            {
                var line = new Line() {Stroke = LineBrush, StrokeThickness = GridLineThickness};
                Border.Add(line);
                LogicalChildren.Add(line);
                VisualChildren.Add(line);
            }

            Panel = new GridPanel(this);
            LogicalChildren.Add(Panel);
            VisualChildren.Add(Panel);
        }

        protected void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Controller.SetItemsSource(ItemsSource);
            Panel.RecreateContent();
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureBorder();
            Panel.Measure(new Size(availableSize.Width - 2.0 * GridLineThickness, availableSize.Height - 2.0 * GridLineThickness));
            return Panel.DesiredSize;

            void MeasureBorder()
            {
                Border[0].StartPoint = new Point(GridLineThickness / 2.0, 0.0);
                Border[0].EndPoint = new Point(GridLineThickness / 2.0, availableSize.Height);
                Border[1].StartPoint = new Point(availableSize.Width - GridLineThickness / 2.0, 0.0);
                Border[1].EndPoint = new Point(availableSize.Width - GridLineThickness / 2.0, availableSize.Height);
                Border[2].StartPoint = new Point(GridLineThickness, GridLineThickness / 2.0);
                Border[2].EndPoint = new Point(availableSize.Width - GridLineThickness, GridLineThickness / 2.0);
                Border[3].StartPoint = new Point(GridLineThickness, availableSize.Height - GridLineThickness / 2.0);
                Border[3].EndPoint = new Point(availableSize.Width - GridLineThickness, availableSize.Height - GridLineThickness / 2.0);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Panel.Arrange(new Rect(GridLineThickness, GridLineThickness,
                finalSize.Width - 2.0 * GridLineThickness, finalSize.Height - 2.0 * GridLineThickness));
            return finalSize;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled)
            {
                switch (e.Key)
                {
                    case Key.Up:
                        FocusPreviousRow();
                        e.Handled = true;
                        break;
                    case Key.Down:
                        FocusNextRow();
                        e.Handled = true;
                        break;
                    case Key.PageUp:
                        FocusPreviousPageFirstRow();
                        e.Handled = true;
                        break;
                    case Key.PageDown:
                        FocusNextPageLastRow();
                        e.Handled = true;
                        break;
                    case Key.Home:
                        FocusFirstRow();
                        e.Handled = true;
                        break;
                    case Key.End:
                        FocusLastRow();
                        e.Handled = true;
                        break;
                }
            }
            base.OnKeyDown(e);
        }

        internal void UpdateSelection(Cell cell)
        {
            switch (SelectionType)
            {
                case SelectionType.Single:
                    SelectedRows.Clear();
                    SelectedRows.Add(new SelectedPair() { From = cell.Row, To = cell.Row });
                    break;
                case SelectionType.Multiple:
                    bool rowWasSelected = false;
                    for (int i = 0; i < SelectedRows.Count; i++)
                    {
                        var selectedRow = SelectedRows[i];
                        if (selectedRow.From <= cell.Row && cell.Row <= selectedRow.To)
                        {
                            rowWasSelected = true;
                            if (selectedRow.From == selectedRow.To)
                            {
                                SelectedRows.RemoveAt(i);
                            }
                            else
                            {
                                if (selectedRow.From < cell.Row && cell.Row < selectedRow.To)
                                {
                                    SelectedRows.Insert(i + 1,
                                        new SelectedPair() { From = cell.Row + 1, To = selectedRow.To });
                                    selectedRow.To = cell.Row - 1;
                                }
                                else
                                {
                                    if (selectedRow.From == cell.Row)
                                    {
                                        selectedRow.From++;
                                    }
                                    else //cell.Row == selectedRow.To
                                    {
                                        selectedRow.To--;
                                    }
                                }
                            }

                            break;
                        }
                    }

                    if (!rowWasSelected)
                    {
                        if (SelectedRows.Count > 0)
                        {
                            for (int i = 0; i < SelectedRows.Count; i++)
                            {
                                var selectedRow = SelectedRows[i];

                                if (cell.Row == selectedRow.From - 1)
                                {
                                    selectedRow.From--;
                                    break;
                                }

                                if (cell.Row == selectedRow.To + 1)
                                {
                                    selectedRow.To++;
                                    if (i < SelectedRows.Count - 1 && SelectedRows[i + 1].From == selectedRow.To + 1)
                                    {
                                        selectedRow.To = SelectedRows[i + 1].To;
                                        SelectedRows.RemoveAt(i + 1);
                                    }

                                    break;
                                }

                                if (cell.Row < selectedRow.From)
                                {
                                    SelectedRows.Insert(i, new SelectedPair() { From = cell.Row, To = cell.Row });
                                    break;
                                }
                            }

                            if (cell.Row > SelectedRows[SelectedRows.Count - 1].To)
                            {
                                SelectedRows.Add(new SelectedPair() { From = cell.Row, To = cell.Row });
                            }
                        }
                        else
                        {
                            SelectedRows.Add(new SelectedPair() { From = cell.Row, To = cell.Row });
                        }
                    }

                    break;
            }

            Panel.UpdateSelection();
        }

        internal bool IsSelected(int row)
        {
            foreach (var selectedRow in SelectedRows)
            {
                if (selectedRow.From <= row && row <= selectedRow.To)
                    return true;
            }

            return false;
        }

        public void SelectRow(int row)
        {
            SelectedRows.Clear();
            SelectedRows.Add(new SelectedPair() { From = row, To = row });
            Panel.UpdateSelection();
        }

        public void FocusPreviousRow()
        {
            if (FocusedRow > 0)
            {
                FocusRow(FocusedRow - 1);
            }
        }

        public void FocusNextRow()
        {
            if (FocusedRow >= 0 && FocusedRow < Controller.Count - 1)
            {
                FocusRow(FocusedRow + 1);
            }
        }

        public void FocusPreviousPageFirstRow()
        {
            //
        }

        public void FocusNextPageLastRow()
        {
            //
        }

        public void FocusFirstRow()
        {
            if (FocusedRow >= 0)
            {
                FocusRow(0);
            }
        }

        public void FocusLastRow()
        {
            if (FocusedRow >= 0)
            {
                FocusRow(Controller.Count - 1);
            }
        }

        public void FocusRow(int row)
        {
            SelectRow(row);
            Panel.FocusRow(row);
        }

        public void ScrollLineUp()
        {
            Panel.ScrollLineUp();
        }

        public void ScrollLineDown()
        {
            Panel.ScrollLineDown();
        }
    }
}
