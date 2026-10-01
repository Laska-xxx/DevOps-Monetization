using Core.Board;

namespace Core.Commands
{
    public class EraseCellCommand : ICellCommand
    {
        private readonly BoardModel _board;
        private readonly int _row;
        private readonly int _col;

        private int _previousValue;
        private HashSetSnapshot _previousNotes;

        public EraseCellCommand(BoardModel board, int row, int col)
        {
            _board = board;
            _row = row;
            _col = col;
        }

        public void Execute()
        {
            _previousValue = _board.GetValue(_row, _col);
            _previousNotes = _board.GetNotesSnapshot(_row, _col);

            _board.SetValue(_row, _col, 0);

            _board.ClearNotes(_row, _col);
        }

        public void Undo()
        {
            _board.SetValue(_row, _col, _previousValue);
            _board.SetNotes(_row, _col, _previousNotes);
        }
    }
}
