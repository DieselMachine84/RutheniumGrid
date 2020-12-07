using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ruthenium.TestApp
{
    public partial class CustomDraw : UserControl
    {
        public CustomDraw()
        {
            InitializeComponent();
            
            DataContext = DataSources.GetSimpleDataSource(100);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
