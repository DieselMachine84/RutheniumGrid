using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Ruthenium.DataGrid;

namespace Ruthenium.TestApp
{
    public partial class LargeAndSmallRows : UserControl
    {
        public LargeAndSmallRows()
        {
            InitializeComponent();
            
            DataContext = DataSources.GetSimpleDataSource(1000);
        }
        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }

    public class ShapeTemplateSelector : DataTemplateSelector
    {
        public override IDataTemplate SelectTemplate(object item, AvaloniaObject container)
        {
            RectData rectData = item as RectData;
            if (rectData == null)
                return null;

            GridCell cell = (GridCell) container;
            int width = Convert.ToInt32(rectData.Width);
            if (width % 10 == 0)
                return cell.FindResource("ellipse") as IDataTemplate;
            else
                return cell.FindResource("rect") as IDataTemplate;
        }
    }
}
