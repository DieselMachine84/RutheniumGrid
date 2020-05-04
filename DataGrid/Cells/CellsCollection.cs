using System;
using System.Collections.Generic;
using System.Text;

namespace Ruthenium.DataGrid
{
    internal class CellsCollection : List<Cell>
    {
        private const int NewRowsCreationCount = 8;

        private readonly List<Column> _columns;
        private readonly Action<Cell> _addCellAction;
        private readonly Action<Cell> _removeCellAction;
        private int _firstRowCellIndex;

        private void AddEmptyRows()
        {
            for (int i = 0; i < NewRowsCreationCount; i++)
            {
                foreach (var column in _columns)
                {
                    var cell = new Cell(column);
                    if (_firstRowCellIndex > 0)
                    {
                        Insert(_firstRowCellIndex, cell);
                        _firstRowCellIndex++;
                    }
                    else
                    {
                        Add(cell);
                    }
                    _addCellAction(cell);
                }
            }
        }

        private void RemoveEmptyRows(int optimalRowsCount)
        {
            while (Count > optimalRowsCount * _columns.Count)
            {
                //TODO remove not previous but less used row
                int indexToRemoveAt = (_firstRowCellIndex > 0) ? _firstRowCellIndex - 1 : Count - 1;
                var cell = this[indexToRemoveAt];
                RemoveAt(indexToRemoveAt);
                _removeCellAction(cell);
                if (_firstRowCellIndex > 0)
                {
                    _firstRowCellIndex--;
                }
            }
        }

        public CellsCollection(List<Column> columns, Action<Cell> addCellAction, Action<Cell> removeCellAction)
        {
            _columns = columns;
            _addCellAction = addCellAction;
            _removeCellAction = removeCellAction;
            AddEmptyRows();
            _firstRowCellIndex = 0;
        }

        public int GetFirstRowCellIndex(int firstRow)
        {
            int oldFirstRow = this[_firstRowCellIndex].Row;
            if (oldFirstRow == -1)
            {
                _firstRowCellIndex = 0;
                return _firstRowCellIndex;
            }

            if (firstRow != oldFirstRow)
            {
                if (firstRow < oldFirstRow)
                {
                    if ((oldFirstRow - firstRow) * _columns.Count < Count)
                    {
                        _firstRowCellIndex -= (oldFirstRow - firstRow) * _columns.Count;
                        if (_firstRowCellIndex < 0)
                            _firstRowCellIndex += Count;
                    }
                    else
                    {
                        _firstRowCellIndex = 0;
                    }
                }
                else
                {
                    if ((firstRow - oldFirstRow) * _columns.Count < Count)
                    {
                        _firstRowCellIndex += (firstRow - oldFirstRow) * _columns.Count;
                        if (_firstRowCellIndex >= Count)
                            _firstRowCellIndex -= Count;
                    }
                    else
                    {
                        _firstRowCellIndex = 0;
                    }
                }
            }

            return _firstRowCellIndex;
        }

        public int GetRowCellIndex(int row)
        {
            int firstRow = this[_firstRowCellIndex].Row;
            while ((row - firstRow) * _columns.Count >= Count)
            {
                AddEmptyRows();
            }

            int rowCellIndex = _firstRowCellIndex + (row - firstRow) * _columns.Count;
            if (rowCellIndex >= Count)
                rowCellIndex -= Count;
            return rowCellIndex;
        }

        public void OptimizeFreeCells(int beyondLastRow)
        {
            int firstRow = this[_firstRowCellIndex].Row;
            RemoveEmptyRows(beyondLastRow - firstRow + 4 * NewRowsCreationCount);

            int freeCellIndex = _firstRowCellIndex + (beyondLastRow - firstRow) * _columns.Count;
            if (freeCellIndex >= Count)
                freeCellIndex -= Count;
            while (freeCellIndex != _firstRowCellIndex)
            {
                this[freeCellIndex].IsVisible = false;
                freeCellIndex++;
                if (freeCellIndex == Count)
                    freeCellIndex = 0;
            }
        }

        public override string ToString()
        {
            if (_columns.Count == 0)
                return String.Empty;
            
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i % _columns.Count == 0)
                    result.AppendLine();
                result.Append(this[i]);
                result.Append("    ");
            }
            return result.ToString().Trim();
        }
    }
}