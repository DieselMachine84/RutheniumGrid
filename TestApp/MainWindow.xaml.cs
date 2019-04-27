using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Ruthenium.TestApp
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = DataSources.GetSimpleDataSource(1000000);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}