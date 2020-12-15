using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Ruthenium.DataGrid
{
    public class DataGrid : TemplatedControl
    {
        internal static SolidColorBrush SelectedCellBrush { get; } = new SolidColorBrush {Color = Color.FromRgb(0x11, 0x9E, 0xDA)};

        public static readonly StyledProperty<IBrush> DataAreaBackgroundProperty =
            AvaloniaProperty.Register<DataGrid, IBrush>(nameof(DataAreaBackground));

        public static readonly StyledProperty<bool> ShowHorizontalLinesProperty =
            AvaloniaProperty.Register<DataGrid, bool>(nameof(ShowHorizontalLines), defaultValue: true);

        public static readonly StyledProperty<IBrush> HorizontalLinesBrushProperty =
            AvaloniaProperty.Register<DataGrid, IBrush>(nameof(HorizontalLinesBrush),
                defaultValue: new SolidColorBrush {Color = Colors.Black});

        public static readonly StyledProperty<double> HorizontalLinesThicknessProperty =
            AvaloniaProperty.Register<DataGrid, double>(nameof(HorizontalLinesThickness), defaultValue: 1.0);

        public static readonly StyledProperty<bool> ShowVerticalLinesProperty =
            AvaloniaProperty.Register<DataGrid, bool>(nameof(ShowVerticalLines), defaultValue: true);

        public static readonly StyledProperty<IBrush> VerticalLinesBrushProperty =
            AvaloniaProperty.Register<DataGrid, IBrush>(nameof(VerticalLinesBrush),
                defaultValue: new SolidColorBrush {Color = Colors.Black});

        public static readonly StyledProperty<double> VerticalLinesThicknessProperty =
            AvaloniaProperty.Register<DataGrid, double>(nameof(VerticalLinesThickness), defaultValue: 1.0);

        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource;

        private GridPanel Panel { get; }

        internal DataController Controller { get; } = new DataController();

        public IBrush DataAreaBackground
        {
            get => GetValue(DataAreaBackgroundProperty);
            set => SetValue(DataAreaBackgroundProperty, value);
        }

        public bool ShowHorizontalLines
        {
            get => GetValue(ShowHorizontalLinesProperty);
            set => SetValue(ShowHorizontalLinesProperty, value);
        }

        public IBrush HorizontalLinesBrush
        {
            get => GetValue(HorizontalLinesBrushProperty);
            set => SetValue(HorizontalLinesBrushProperty, value);
        }

        public double HorizontalLinesThickness
        {
            get => GetValue(HorizontalLinesThicknessProperty);
            set => SetValue(HorizontalLinesThicknessProperty, value);
        }

        public bool ShowVerticalLines
        {
            get => GetValue(ShowVerticalLinesProperty);
            set => SetValue(ShowVerticalLinesProperty, value);
        }

        public IBrush VerticalLinesBrush
        {
            get => GetValue(VerticalLinesBrushProperty);
            set => SetValue(VerticalLinesBrushProperty, value);
        }

        public double VerticalLinesThickness
        {
            get => GetValue(VerticalLinesThicknessProperty);
            set => SetValue(VerticalLinesThicknessProperty, value);
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
            Panel.Measure(availableSize);
            return Panel.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Panel.Arrange(new Rect(0.0, 0.0, finalSize.Width, finalSize.Height));
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
