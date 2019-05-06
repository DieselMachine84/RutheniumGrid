using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ruthenium.TestApp
{
    public partial class SimpleGrid : UserControl
    {
        public SimpleGrid()
        {
            InitializeComponent();
            
            DataContext = DataSources.GetSimpleDataSource(1000);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
