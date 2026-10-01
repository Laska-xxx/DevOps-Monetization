using System.Collections.Generic;

namespace Core.Board
{
    public class CellModel
    {
        public int Row { get; }
        public int Col { get; }
        public int Value { get; set; }
        public bool IsFixed { get; set; }

        private readonly HashSet<int> _notes = new HashSet<int>();
        public IReadOnlyCollection<int> Notes => _notes;

        public CellModel(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public void AddNote(int value) => _notes.Add(value);
        public void RemoveNote(int value) => _notes.Remove(value);
        public bool HasNote(int value) => _notes.Contains(value);
        public void ClearNotes() => _notes.Clear();

        public HashSet<int> SnapshotNotes() => new HashSet<int>(_notes);

        public void RestoreNotes(HashSet<int> snapshot)
        {
            _notes.Clear();
            if (snapshot == null) return;
            foreach (var n in snapshot) _notes.Add(n);
        }
    }
}
