using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Ruthenium.DataGrid;

namespace Ruthenium.TestApp
{
    public class RectData
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public string Text => $"{Width}X{Height}";
        public override string ToString()
        {
            return Text;
        }
    }

    public class GridData
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public RectData RectData { get; set; }
        public override string ToString()
        {
            return $"{Id}   {Text}";
        }
    }


    public static class DataSources
    {
        public static List<GridData> GetSimpleDataSource(int n)
        {
            Random random = new Random();
            const int NumLetters = 26;
            List<GridData> dataSource = new List<GridData>(n);
            for (int i = 0; i < n; i++)
            {
                StringBuilder text = new StringBuilder(4);
                int quotient = i;
                int remainder = 0;
                do
                {
                    remainder = quotient % NumLetters;
                    quotient = quotient / NumLetters - 1;
                    text.Insert(0, (char) (65 + remainder));
                } while (quotient >= 0);
                dataSource.Add(new GridData()
                {
                    Id = i,
                    Text = text.ToString(),
                    RectData = new RectData()
                    {
                        Width = random.Next(10, 300),
                        Height = random.Next(10, 200)
                    }
                });
            }
            return dataSource;
        }
    }
}