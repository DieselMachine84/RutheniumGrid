using System;
using System.Collections.Generic;
using System.Text;

namespace Ruthenium.DataGrid
{
    internal class CellsCollection
    {
        private enum AddEmptyRowsMode { BeforeInitial, AfterInitial }
        private const int NewRowsCreationCount = 8;

        private List<Cell> _cells = new List<Cell>();
        private readonly List<Column> _columns;
        private readonly Action<Cell> _addCellAction;
        private readonly Action<Cell> _removeCellAction;
        private int _initialRowCellIndex;

        private void AddEmptyRows(AddEmptyRowsMode mode)
        {
            for (int i = 0; i < NewRowsCreationCount; i++)
            {
                foreach (var column in _columns)
                {
                    var cell = new Cell(column);
                    switch (mode)
                    {
                        case AddEmptyRowsMode.BeforeInitial:
                            if (_initialRowCellIndex > 0)
                            {
                                _cells.Insert(_initialRowCellIndex, cell);
                                _initialRowCellIndex++;
                            }
                            else
                            {
                                _cells.Add(cell);
                            }

                            break;
                        case AddEmptyRowsMode.AfterInitial:
                            _cells.Insert(_initialRowCellIndex + _columns.Count, cell);
                            break;
                    }
                    _addCellAction(cell);
                }
            }
        }

        private void RemoveEmptyRows(int optimalRowsCount)
        {
            while (_cells.Count > optimalRowsCount * _columns.Count)
            {
                //TODO remove not previous but less used row
                int indexToRemoveAt = (_initialRowCellIndex > 0) ? _initialRowCellIndex - 1 : _cells.Count - 1;
                var cell = _cells[indexToRemoveAt];
                _cells.RemoveAt(indexToRemoveAt);
                _removeCellAction(cell);
                if (_initialRowCellIndex > 0)
                {
                    _initialRowCellIndex--;
                }
            }
        }

        public CellsCollection(List<Column> columns, Action<Cell> addCellAction, Action<Cell> removeCellAction)
        {
            _columns = columns;
            _addCellAction = addCellAction;
            _removeCellAction = removeCellAction;
            AddEmptyRows(AddEmptyRowsMode.BeforeInitial);
            _initialRowCellIndex = 0;
        }

        public int GetInitialRow()
        {
            return _cells[_initialRowCellIndex].Row;
        }
        
        public void SetInitialRow(int initialRow)
        {
            int oldInitialRow = GetInitialRow();
            if (oldInitialRow == -1)
            {
                _initialRowCellIndex = 0;
            }

            if (initialRow != oldInitialRow)
            {
                if (initialRow < oldInitialRow)
                {
                    if ((oldInitialRow - initialRow) * _columns.Count < _cells.Count)
                    {
                        _initialRowCellIndex -= (oldInitialRow - initialRow) * _columns.Count;
                        if (_initialRowCellIndex < 0)
                            _initialRowCellIndex += _cells.Count;
                    }
                    else
                    {
                        _initialRowCellIndex = 0;
                    }
                }
                else
                {
                    if ((initialRow - oldInitialRow) * _columns.Count < _cells.Count)
                    {
                        _initialRowCellIndex += (initialRow - oldInitialRow) * _columns.Count;
                        if (_initialRowCellIndex >= _cells.Count)
                            _initialRowCellIndex -= _cells.Count;
                    }
                    else
                    {
                        _initialRowCellIndex = 0;
                    }
                }
            }
        }

        public Cell GetCell(int initialRowDiff, int column)
        {
            while ((Math.Abs(initialRowDiff) + 1) * _columns.Count >= _cells.Count)
            {
                AddEmptyRows((initialRowDiff >= 0) ? AddEmptyRowsMode.BeforeInitial : AddEmptyRowsMode.AfterInitial);
            }
            int rowCellIndex = _initialRowCellIndex + initialRowDiff * _columns.Count;
            if (rowCellIndex < 0)
                rowCellIndex += _cells.Count;
            if (rowCellIndex >= _cells.Count)
                rowCellIndex -= _cells.Count;
            return _cells[rowCellIndex + column];
        }

        /*public int GetRowCellIndex(int row)
        {
            int firstRow = GetInitialRow();
            while ((Math.Abs(row - firstRow) + 1) * _columns.Count >= _cells.Count)
            {
                AddEmptyRows((row > firstRow) ? AddEmptyRowsMode.BeforeInitial : AddEmptyRowsMode.AfterInitial);
            }

            int rowCellIndex = _initialRowCellIndex + (row - firstRow) * _columns.Count;
            if (rowCellIndex < 0)
                rowCellIndex += _cells.Count;
            if (rowCellIndex >= _cells.Count)
                rowCellIndex -= _cells.Count;
            return rowCellIndex;
        }*/

        public void UpdateVisibility(int beyondLastRow)
        {
            int firstRow = GetInitialRow();
            int notVisibleCellIndex = _initialRowCellIndex + (beyondLastRow - firstRow) * _columns.Count;
            if (notVisibleCellIndex >= _cells.Count)
                notVisibleCellIndex -= _cells.Count;
            while (notVisibleCellIndex != _initialRowCellIndex)
            {
                _cells[notVisibleCellIndex].IsVisible = false;
                notVisibleCellIndex++;
                if (notVisibleCellIndex == _cells.Count)
                    notVisibleCellIndex = 0;
            }
        }
        public void OptimizeFreeCells(int beyondLastRow)
        {
            int firstRow = GetInitialRow();
            RemoveEmptyRows(beyondLastRow - firstRow + 4 * NewRowsCreationCount);
        }

        public IEnumerable<Cell> GetVisibleCells()
        {
            int index = _initialRowCellIndex;
            while (true)
            {
                var cell = _cells[index];
                if (cell.IsVisible)
                    yield return cell;
                else
                    yield break;
                index++;
                if (index == _cells.Count)
                    index = 0;
                if (index == _initialRowCellIndex)
                    yield break;
            }
        }

        public override string ToString()
        {
            if (_columns.Count == 0)
                return String.Empty;
            
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < _cells.Count; i++)
            {
                if (i % _columns.Count == 0)
                    result.AppendLine();
                result.Append(_cells[i]);
                result.Append("    ");
            }
            return result.ToString().Trim();
        }
    }
}
