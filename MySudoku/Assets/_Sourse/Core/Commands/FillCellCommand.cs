using Core.Board;

namespace Core.Commands
{
    public class FillCellCommand : ICellCommand
    {
        private readonly BoardModel _board;
        private readonly int _row;
        private readonly int _col;
        private readonly int _newValue;

        private int _previousValue;
        private HashSetSnapshot _previousNotes;

        public FillCellCommand(BoardModel board, int row, int col, int newValue)
        {
            _board = board;
            _row = row;
            _col = col;
            _newValue = newValue;
        }

        public void Execute()
        {
            _previousValue = _board.GetValue(_row, _col);
            _previousNotes = _board.GetNotesSnapshot(_row, _col);

            _board.SetValue(_row, _col, _newValue);
            _board.ClearNotes(_row, _col);
        }

        public void Undo()
        {
            _board.SetValue(_row, _col, _previousValue);
            _board.SetNotes(_row, _col, _previousNotes);
        }
    }
}
