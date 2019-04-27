using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace Ruthenium.DataGrid
{
    public class GridTemplateColumn : GridColumn
    {
        public static readonly StyledProperty<IDataTemplate> CellTemplateProperty =
            AvaloniaProperty.Register<GridColumn, IDataTemplate>(nameof(CellTemplate));

        public IDataTemplate CellTemplate
        {
            get => GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        public override IControl CreateControl()
        {
            var dataTemplate = CellTemplate ?? FuncDataTemplate.Default;
            return dataTemplate.Build(null);
        }
    }
}