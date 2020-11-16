using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Xunit;

namespace Ruthenium.DataGrid.UnitTests
{
    public class DataGridFixture : IDisposable
    {
        private readonly Thread uiThread;
        public ClassicDesktopStyleApplicationLifetime ApplicationLifetime { get; private set; }

        private void UIThreadFunc()
        {
            ApplicationLifetime = new ClassicDesktopStyleApplicationLifetime();
            ApplicationLifetime.ShutdownMode = ShutdownMode.OnMainWindowClose;
            AppBuilder.Configure<App>().UsePlatformDetect().SetupWithLifetime(ApplicationLifetime);
            ApplicationLifetime.Start(Array.Empty<string>());
        }

        public DataGridFixture()
        {
            uiThread = new Thread(UIThreadFunc);
            uiThread.Start();
        }

        public void Dispose()
        {
            Dispatcher.UIThread.Post(() =>
            {
                ApplicationLifetime.MainWindow.Close();
                ApplicationLifetime.Dispose();
            });
            uiThread.Join();
        }
    }

    [CollectionDefinition("DataGrid collection")]
    public class TestCollection : ICollectionFixture<DataGridFixture>
    {
    }
}