using System;
using System.Collections.Generic;
using System.Text;

namespace Ruthenium.DataGrid
{
    internal class GridCells : List<GridCell>
    {
        private const int NewRowsCreationCount = 8;

        private readonly int _columnsCount;
        private readonly Action<GridCell> _newCellPanelAction;
        private int _firstRowCellIndex;

        private void AddEmptyRows()
        {
            for (int i = 0; i < NewRowsCreationCount; i++)
            {
                for (int column = 0; column < _columnsCount; column++)
                {
                    var cell = new GridCell(column);
                    if (_firstRowCellIndex > 0)
                    {
                        Insert(_firstRowCellIndex, cell);
                        _firstRowCellIndex++;
                    }
                    else
                    {
                        Add(cell);
                    }
                    _newCellPanelAction(cell);
                }
            }
        }

        private void RemoveEmptyRows(int optimalRowsCount)
        {
            while (Count > optimalRowsCount * _columnsCount)
            {
                if (_firstRowCellIndex > 0)
                {
                    _firstRowCellIndex--;
                    RemoveAt(_firstRowCellIndex);
                }
                else
                {
                    RemoveAt(Count - 1);
                }
            }
        }

        public GridCells(int columnsCount, Action<GridCell> newCellPanelAction)
        {
            _columnsCount = columnsCount;
            _newCellPanelAction = newCellPanelAction;
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
                    if ((oldFirstRow - firstRow) * _columnsCount < Count)
                    {
                        _firstRowCellIndex -= (oldFirstRow - firstRow) * _columnsCount;
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
                    if ((firstRow - oldFirstRow) * _columnsCount < Count)
                    {
                        _firstRowCellIndex += (firstRow - oldFirstRow) * _columnsCount;
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
            while ((row - firstRow) * _columnsCount >= Count)
            {
                AddEmptyRows();
            }

            int rowCellIndex = _firstRowCellIndex + (row - firstRow) * _columnsCount;
            if (rowCellIndex >= Count)
                rowCellIndex -= Count;
            return rowCellIndex;
        }

        public void OptimizeFreeCells(int beyondLastRow)
        {
            int firstRow = this[_firstRowCellIndex].Row;
            if (beyondLastRow - firstRow < NewRowsCreationCount)
                AddEmptyRows();
            RemoveEmptyRows(beyondLastRow - firstRow + 4 * NewRowsCreationCount);

            int freeCellIndex = _firstRowCellIndex + (beyondLastRow - firstRow) * _columnsCount;
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
            if (_columnsCount == 0)
                return String.Empty;
            
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < Count; i++)
            {
                if (i % _columnsCount == 0)
                    result.AppendLine();
                result.Append(this[i]);
                result.Append("    ");
            }
            return result.ToString().Trim();
        }
    }
}