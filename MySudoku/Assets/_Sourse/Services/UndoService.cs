using System.Collections.Generic;
using Core.Commands;

namespace Services
{
    public interface IUndoService
    {
        bool CanUndo { get; }
        void Execute(ICellCommand command);
        void UndoLast();
        void Clear();
    }

    public class UndoService : IUndoService
    {
        private const int MaxHistory = 10;
        private readonly LinkedList<ICellCommand> _history = new LinkedList<ICellCommand>();

        public bool CanUndo => _history.Count > 0;

        public void Execute(ICellCommand command)
        {
            command.Execute();
            _history.AddLast(command);

            if (_history.Count > MaxHistory)
                _history.RemoveFirst();
        }

        public void UndoLast()
        {
            if (!CanUndo) return;

            var last = _history.Last.Value;
            last.Undo();
            _history.RemoveLast();
        }

        public void Clear() => _history.Clear();
    }
}
