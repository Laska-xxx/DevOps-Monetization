using System;
using System.Collections.Generic;
using Zenject;
using Core.Board;
using Signals;

namespace Services
{
    public interface IHintService
    {
        bool HasFreeHint { get; }
        void ResetForNewGame();
        bool TryUseHint(BoardModel board, out int row, out int col, out int value);
        void GrantRewardedHint();
    }

    public class HintService : IHintService
    {
        private readonly SignalBus _signalBus;
        private readonly Random _rng = new Random();

        private bool _freeHintUsed;
        private int _adHints;

        public bool HasFreeHint => !_freeHintUsed;

        public HintService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void ResetForNewGame()
        {
            _freeHintUsed = false;
            _adHints = 0;
        }

        public bool TryUseHint(BoardModel board, out int row, out int col, out int value)
        {
            row = col = value = -1;

            if (!HasFreeHint)
            {
                if (_adHints <= 0) return false;
                _adHints--;
            }

            var emptyCells = new List<(int row, int col)>();
            int n = board.SideLength;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (board.GetValue(r, c) == 0)
                        emptyCells.Add((r, c));

            if (emptyCells.Count == 0)
                return false;

            var chosen = emptyCells[_rng.Next(emptyCells.Count)];
            row = chosen.row;
            col = chosen.col;
            value = board.Solution[row * n + col];

            board.SetValue(row, col, value);

            bool wasFree = !_freeHintUsed;
            _freeHintUsed = true;

            _signalBus.Fire(new HintUsedSignal { WasFree = wasFree });

            return true;
        }

        public void GrantRewardedHint()
        {
            _adHints++;
        }
    }
}
