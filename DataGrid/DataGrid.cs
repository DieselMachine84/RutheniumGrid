using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Ruthenium
{
    public class DataGrid : Control
    {
        static DataGrid()
        {
            ClipToBoundsProperty.OverrideDefaultValue<DataGrid>(true);
        }

        public override void Render(DrawingContext context)
        {
            FormattedText text = new FormattedText
            {
                Typeface = new Typeface(FontFamily.Default, 14.0, FontStyle.Normal, FontWeight.Bold),
                Text = "Ruthenium grid",
                TextAlignment = TextAlignment.Left,
                Wrapping = TextWrapping.NoWrap,
            };
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(255, 127, 0, 0));
            context.DrawText(brush, new Point(0.0, 0.0), text);
        }
    }
}