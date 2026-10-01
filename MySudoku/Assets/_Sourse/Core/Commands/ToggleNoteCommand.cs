using Core.Board;

namespace Core.Commands
{
    public class ToggleNoteCommand : ICellCommand
    {
        private readonly BoardModel _board;
        private readonly int _row;
        private readonly int _col;
        private readonly int _value;

        public ToggleNoteCommand(BoardModel board, int row, int col, int value)
        {
            _board = board;
            _row = row;
            _col = col;
            _value = value;
        }

        public void Execute() => _board.ToggleNote(_row, _col, _value);

        public void Undo() => _board.ToggleNote(_row, _col, _value);
    }
}
