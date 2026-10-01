using Core.Board;

namespace Services
{
    public interface INotesService
    {
        bool NotesModeActive { get; }
        void ToggleNotesMode();
        void CleanupPeerNotes(BoardModel board, int row, int col, int value, int boxWidth, int boxHeight);
    }

    public class NotesService : INotesService
    {
        public bool NotesModeActive { get; private set; }

        public void ToggleNotesMode() => NotesModeActive = !NotesModeActive;

        public void CleanupPeerNotes(BoardModel board, int row, int col, int value, int boxWidth, int boxHeight)
        {
            int n = board.SideLength;

            for (int i = 0; i < n; i++)
            {
                RemoveNoteIfPresent(board, row, i, value);
                RemoveNoteIfPresent(board, i, col, value);
            }

            if (boxWidth > 0 && boxHeight > 0)
            {
                int boxRow = (row / boxHeight) * boxHeight;
                int boxCol = (col / boxWidth) * boxWidth;

                for (int r = 0; r < boxHeight; r++)
                    for (int c = 0; c < boxWidth; c++)
                        RemoveNoteIfPresent(board, boxRow + r, boxCol + c, value);
            }
        }

        private void RemoveNoteIfPresent(BoardModel board, int row, int col, int value)
        {
            var cell = board.GetCell(row, col);
            if (cell.HasNote(value)) cell.RemoveNote(value);
        }
    }
}
