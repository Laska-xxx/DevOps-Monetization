using System;

namespace Core.Board
{
    public class BoardModel
    {
        public BoardSize Size { get; }
        public int SideLength { get; }

        public byte[] Solution { get; }

        private readonly CellModel[,] _cells;

        private BoardModel(BoardSize size, byte[] solution)
        {
            Size = size;
            SideLength = (int)size;
            Solution = solution;

            _cells = new CellModel[SideLength, SideLength];
            for (int r = 0; r < SideLength; r++)
                for (int c = 0; c < SideLength; c++)
                    _cells[r, c] = new CellModel(r, c);
        }

        public static BoardModel FromSolutionAndPuzzle(BoardSize size, byte[] solution, byte[] puzzle)
        {
            if (solution.Length != (int)size * (int)size)
                throw new ArgumentException("Некорректная длина массива решения для указанного размера доски");

            var board = new BoardModel(size, solution);
            int n = board.SideLength;

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    byte value = puzzle[r * n + c];
                    var cell = board._cells[r, c];
                    if (value != 0)
                    {
                        cell.Value = value;
                        cell.IsFixed = true;
                    }
                }
            }

            return board;
        }

        public static BoardModel FromSave(BoardSize size, byte[] solution, byte[] currentValues, bool[] isFixed)
        {
            var board = new BoardModel(size, solution);
            int n = board.SideLength;

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    int index = r * n + c;
                    var cell = board._cells[r, c];
                    cell.Value = currentValues[index];
                    cell.IsFixed = isFixed[index];
                }
            }

            return board;
        }

        public CellModel GetCell(int row, int col) => _cells[row, col];
        public int GetValue(int row, int col) => _cells[row, col].Value;
        public void SetValue(int row, int col, int value) => _cells[row, col].Value = value;
        public bool IsFixed(int row, int col) => _cells[row, col].IsFixed;

        public HashSetSnapshot GetNotesSnapshot(int row, int col) =>
            new HashSetSnapshot(_cells[row, col].SnapshotNotes());

        public void SetNotes(int row, int col, HashSetSnapshot snapshot) =>
            _cells[row, col].RestoreNotes(snapshot.Values);

        public void ClearNotes(int row, int col) => _cells[row, col].ClearNotes();

        public void ToggleNote(int row, int col, int value)
        {
            var cell = _cells[row, col];
            if (cell.HasNote(value)) cell.RemoveNote(value);
            else cell.AddNote(value);
        }

        public bool IsCorrectValue(int row, int col, int value) =>
            Solution[row * SideLength + col] == value;

        public bool IsValueComplete(int value)
        {
            int count = 0;
            for (int r = 0; r < SideLength; r++)
                for (int c = 0; c < SideLength; c++)
                    if (Solution[r * SideLength + c] == value && GetValue(r, c) == value)
                        count++;

            return count == SideLength;
        }

        public byte[] ExportCurrentValues()
        {
            var result = new byte[SideLength * SideLength];
            for (int r = 0; r < SideLength; r++)
                for (int c = 0; c < SideLength; c++)
                    result[r * SideLength + c] = (byte)_cells[r, c].Value;
            return result;
        }

        public bool[] ExportFixedMask()
        {
            var result = new bool[SideLength * SideLength];
            for (int r = 0; r < SideLength; r++)
                for (int c = 0; c < SideLength; c++)
                    result[r * SideLength + c] = _cells[r, c].IsFixed;
            return result;
        }

        public void ClearPlayerEntries()
        {
            for (int r = 0; r < SideLength; r++)
            {
                for (int c = 0; c < SideLength; c++)
                {
                    if (_cells[r, c].IsFixed) continue;

                    _cells[r, c].Value = 0;
                    _cells[r, c].ClearNotes();
                }
            }
        }
    }

    public readonly struct HashSetSnapshot
    {
        public readonly System.Collections.Generic.HashSet<int> Values;
        public HashSetSnapshot(System.Collections.Generic.HashSet<int> values) => Values = values;
    }
}
