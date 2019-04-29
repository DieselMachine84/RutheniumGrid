using Avalonia;
using Avalonia.Controls.Templates;

namespace Ruthenium.DataGrid
{
    public class DataTemplateSelector
    {
        public virtual IDataTemplate SelectTemplate(object item, AvaloniaObject container)
        {
            return null;
        }
    }
}