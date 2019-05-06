using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Ruthenium.DataGrid
{
    public class TextColumn : Column
    {
        public override IControl CreateControl()
        {
            var textBinding = new Binding(null, BindingMode.OneWay);
            var textBlock = new TextBlock {Margin = new Thickness(1.0)};
            textBlock.Bind(TextBlock.TextProperty, textBinding);
            return textBlock;
        }
    }
}