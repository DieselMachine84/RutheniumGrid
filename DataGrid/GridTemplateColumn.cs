using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;

namespace Ruthenium.DataGrid
{
    public class GridTemplateColumn : GridColumn
    {
        public static readonly StyledProperty<IDataTemplate> CellTemplateProperty =
            AvaloniaProperty.Register<GridColumn, IDataTemplate>(nameof(CellTemplate));

        public static readonly DirectProperty<GridTemplateColumn, DataTemplateSelector> CellTemplateSelectorProperty =
            AvaloniaProperty.RegisterDirect<GridTemplateColumn, DataTemplateSelector>(nameof(CellTemplateSelector),
                o => o.CellTemplateSelector, (o, v) => o.CellTemplateSelector = v);

        private DataTemplateSelector _cellTemplateSelector;

        protected internal override bool DynamicCreateControlsForCells
        {
            get => CellTemplateSelector != null;
        }

        public IDataTemplate CellTemplate
        {
            get => GetValue(CellTemplateProperty);
            set => SetValue(CellTemplateProperty, value);
        }

        public DataTemplateSelector CellTemplateSelector
        {
            get => _cellTemplateSelector;
            set => SetAndRaise(CellTemplateSelectorProperty, ref _cellTemplateSelector, value);
        }

        private IControl CreateDefaultControl()
        {
            var textBinding = new Binding(null, BindingMode.OneWay);
            var textBlock = new TextBlock {Margin = new Thickness(1.0)};
            textBlock.Bind(TextBlock.TextProperty, textBinding);
            return textBlock;
        }

        public override IControl CreateControl()
        {
            return (CellTemplate != null) ? CellTemplate.Build(null) : CreateDefaultControl();
        }

        public override IControl DynamicCreateControl(GridCell cell)
        {
            if (CellTemplateSelector == null)
                return CreateDefaultControl();
            var dataTemplate = CellTemplateSelector.SelectTemplate(cell.DataContext, cell);
            return (dataTemplate != null) ? dataTemplate.Build(null) : CreateDefaultControl();
        }
    }
}