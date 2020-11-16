using System;
using System.Collections.Generic;
using System.Text;

namespace Ruthenium.DataGrid.UnitTests
{
    public class SimpleGridData
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public override string ToString()
        {
            return $"{Id}   {Text}";
        }
    }

    public static class DataSources
    {
        public static List<SimpleGridData> GetSimpleDataSource(int n)
        {
            const int NumLetters = 26;
            List<SimpleGridData> dataSource = new List<SimpleGridData>(n);
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
                dataSource.Add(new SimpleGridData()
                {
                    Id = i,
                    Text = text.ToString(),
                });
            }
            return dataSource;
        }
    }
}