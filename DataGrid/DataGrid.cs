using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace Ruthenium
{
    public class DataGrid : UserControl
    {
        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource;
        
        private DataGridPanel Panel { get; set; }

        public object ItemsSource
        {
            get => _itemsSource;
            set => SetAndRaise(ItemsSourceProperty, ref _itemsSource, value);
        }

        public List<DataGridColumn> Columns { get; } = new List<DataGridColumn>();

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
            Panel = new DataGridPanel();
            ScrollViewer scrollViewer = new ScrollViewer();
            scrollViewer.Content = Panel;
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
            Panel.RecreateCells(e.NewValue);
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
            Panel.Columns.AddRange(Columns);
            Panel.RecreateCells(ItemsSource);
        }
    }
}