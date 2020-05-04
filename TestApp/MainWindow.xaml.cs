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

            //TODO remove it?
            //DataContext = DataSources.GetSimpleDataSource(1000000);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}