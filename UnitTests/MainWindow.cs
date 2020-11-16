using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Xunit.Sdk;

namespace Ruthenium.DataGrid.UnitTests
{
    public class MainWindow : Window
    {
        private DispatcherTimer _timer;
        public bool _resized;
        public volatile bool Finished;
        public volatile XunitException Exception;
        public DataGrid DataGrid { get; private set; }
        public Action<Window> Resize { get; private set; }
        public Queue<Action<DataGrid>> Actions { get; private set; }

        public void DoActions(Func<DataGrid> createDataGrid, Action<Window> resize,
            Queue<Action<DataGrid>> actions)
        {
            _resized = false;
            Finished = false;
            Exception = null;
            Content = null;

            DataGrid = createDataGrid();
            Resize = resize;
            Actions = actions;
            Content = DataGrid;
            
            _timer = new DispatcherTimer();
            _timer.Tick += TimerOnTick;
            _timer.Interval = new TimeSpan(0, 0, 0, 1);
            _timer.Start();
        }

        private void Stop()
        {
            _timer?.Stop();
            Finished = true;
        }

        private void TimerOnTick(object sender, EventArgs e)
        {
            if (!_resized)
            {
                Resize(this);
                _resized = true;
            }
            else
            {
                if (Actions.Count > 0)
                {
                    var action = Actions.Dequeue();
                    try
                    {
                        action(DataGrid);
                    }
                    catch (XunitException ex)
                    {
                        Exception = ex;
                        Stop();
                    }
                }
                else
                {
                    Stop();
                }
            }
        }
    }
}