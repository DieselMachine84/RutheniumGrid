using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace Ruthenium.DataGrid
{
    public class DataGrid : UserControl
    {
        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource;
        
        private CellsPanel CellsPanel { get; set; }

        public object ItemsSource
        {
            get => _itemsSource;
            set => SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
        }

        public List<GridColumn> Columns { get; } = new List<GridColumn>();

        static DataGrid()
        {
            ItemsSourceProperty.Changed.AddClassHandler<DataGrid>(x => x.ItemsSourceChanged);
        }

        public DataGrid()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            CellsPanel = new CellsPanel();
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = CellsPanel;
            scrollViewer.TemplateApplied += ScrollViewerOnTemplateApplied;
            Content = scrollViewer;
        }

        private void ScrollViewerOnTemplateApplied(object sender, TemplateAppliedEventArgs e)
        {
            ScrollViewer scrollViewer = (ScrollViewer) sender;
            ScrollContentPresenter presenter = (ScrollContentPresenter) scrollViewer.Presenter;
            presenter.CanHorizontallyScroll = false;
        }

        protected void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            CellsPanel.RecreateCells(e.NewValue);
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
            for (int i = 0; i < Columns.Count; i++)
                Columns[i].Index = i;
            CellsPanel.Columns.AddRange(Columns);
            CellsPanel.RecreateCells(ItemsSource);
        }
    }
}