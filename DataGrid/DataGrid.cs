using System;
using System.Collections;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace Ruthenium
{
    public class DataGrid : Control
    {
        public static readonly DirectProperty<DataGrid, object> ItemsSourceProperty =
            AvaloniaProperty.RegisterDirect<DataGrid, object>(nameof(ItemsSource),
                o => o.ItemsSource, (o, v) => o.ItemsSource = v);

        private object _itemsSource = new AvaloniaList<object>();
        
        protected DataGridPanel Panel { get; }

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
            Panel = new DataGridPanel(Columns);
        }

        protected void ItemsSourceChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Panel.CreateContent(e.NewValue);
        }

        //TODO why this code is within ApplyTemplate?
        public override void ApplyTemplate()
        {
            LogicalChildren.Clear();
            LogicalChildren.Add(Panel);
            VisualChildren.Clear();
            VisualChildren.Add(Panel);
        }
    }
}