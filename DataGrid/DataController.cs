using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Ruthenium.DataGrid
{
    public class DataController
    {
        private object _itemsSource;
        private IList _sourceList;
        private int _count;

        protected object ItemsSource => _itemsSource;

        protected IList SourceList => _sourceList ??= ItemsSource as IList;

        protected List<PropertyInfo> Properties { get; } = new List<PropertyInfo>(16);

        public int Count
        {
            get
            {
                if (_count == 0)
                    _count = SourceList?.Count ?? 0;

                return _count;
            }
        }


        private void ClearCache()
        {
            _itemsSource = null;
            _sourceList = null;
            _count = 0;
            Properties.Clear();
        }


        public void SetItemsSource(object itemsSource)
        {
            ClearCache();
            if (itemsSource == null)
                return;
            
            _itemsSource = itemsSource;

            Type itemsSourceType = ItemsSource.GetType();
            foreach (Type interfaceType in itemsSourceType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    Type argumentType = itemsSourceType.GenericTypeArguments[0];
                    Properties.AddRange(argumentType.GetProperties());
                    break;
                }
            }
        }

        public object GetProperty(int row, string propertyName)
        {
            object rowData = SourceList?[row];
            if (rowData == null)
                return null;
            
            PropertyInfo propertyInfo = Properties.FirstOrDefault(p => p.Name == propertyName);
            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(rowData);
        }
    }
}