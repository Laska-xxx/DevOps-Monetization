using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

using Core.Board;
using Core.Commands;
using Core.Generation;
using Core.Save;
using Core.States;
using Signals;

namespace Services
{
    public class GameSessionController : IGameStateHost, ITickable, IInitializable
    {
        private readonly IBoardGeneratorFactory _generatorFactory;
        private readonly IUndoService _undoService;
        private readonly IScoreService _scoreService;
        private readonly IHealthService _healthService;
        private readonly ITimerService _timerService;
        private readonly INotesService _notesService;
        private readonly IHintService _hintService;
        private readonly ISaveService _saveService;
        private readonly IStatsRepository _statsRepository;
        private readonly ICurrencyService _currencyService;
        private readonly Data.CurrencyRewardSO _currencyReward;
        private readonly SignalBus _signalBus;

        private readonly PlayingState _playingState;
        private readonly PausedState _pausedState;
        private readonly GameOverState _gameOverState;
        private readonly WonState _wonState;

        private IGameState _currentState;

        public BoardModel Board { get; private set; }
        public BoardSize CurrentSize { get; private set; }
        public DifficultyLevel CurrentDifficulty { get; private set; }
        public bool IsExtraMode { get; private set; }
        public bool ChillModeActive { get; private set; }

        public float ElapsedSeconds => _timerService.ElapsedSeconds;

        public int CurrentScore => _scoreService.CurrentScore;
        public int CurrentHealth => _healthService.CurrentHealth;
        public bool HasFreeHint => _hintService.HasFreeHint;
        public bool IsPlaying => _currentState == _playingState;

        float IGameStateHost.ElapsedSeconds
        {
            get => _timerService.ElapsedSeconds;
            set => _timerService.SetElapsed(value);
        }

        bool IGameStateHost.IsBoardSolved =>
            Board != null && _generatorFactory.Get(CurrentSize).IsBoardComplete(Board);

        void IGameStateHost.TransitionTo(IGameState next) => SetState(next);

        public GameSessionController(
            IBoardGeneratorFactory generatorFactory,
            IUndoService undoService,
            IScoreService scoreService,
            IHealthService healthService,
            ITimerService timerService,
            INotesService notesService,
            IHintService hintService,
            ISaveService saveService,
            IStatsRepository statsRepository,
            ICurrencyService currencyService,
            Data.CurrencyRewardSO currencyReward,
            SignalBus signalBus)
        {
            _generatorFactory = generatorFactory;
            _undoService = undoService;
            _scoreService = scoreService;
            _healthService = healthService;
            _timerService = timerService;
            _notesService = notesService;
            _hintService = hintService;
            _saveService = saveService;
            _statsRepository = statsRepository;
            _currencyService = currencyService;
            _currencyReward = currencyReward;
            _signalBus = signalBus;

            _gameOverState = new GameOverState();
            _wonState = new WonState();
            _playingState = new PlayingState(this, _wonState);
            _pausedState = new PausedState();
        }

        public void Initialize() { }

        public void Tick() => _currentState?.Tick(Time.deltaTime);

        public async UniTask StartNewGameAsync(
            BoardSize size,
            DifficultyLevel difficulty,
            bool isExtraMode,
            bool chillMode,
            CancellationToken ct = default)
        {
            CurrentSize = size;
            CurrentDifficulty = difficulty;
            IsExtraMode = isExtraMode;
            ChillModeActive = chillMode;

            var strategy = _generatorFactory.Get(size);
            Board = await strategy.GenerateAsync(difficulty, ct);

            _undoService.Clear();
            _scoreService.Reset();
            _healthService.ChillModeActive = chillMode;
            _healthService.Reset();
            _timerService.Reset();
            _hintService.ResetForNewGame();

            _statsRepository.RegisterGameStarted(size, difficulty, isExtraMode);

            SetState(_playingState);
        }

        public bool TryContinueGame()
        {
            var saveData = _saveService.LoadCurrentGame();
            if (saveData == null) return false;

            var data = saveData.Value;

            CurrentSize = data.Size;
            CurrentDifficulty = data.Difficulty;
            IsExtraMode = data.IsExtraMode;
            ChillModeActive = data.ChillModeActive;

            Board = BoardModel.FromSave(data.Size, data.SolutionCells, data.CurrentCells, data.IsFixedCell);

            int n = Board.SideLength;
            foreach (var note in data.Notes)
            {
                int row = note.CellIndex / n;
                int col = note.CellIndex % n;
                for (int v = 1; v <= n; v++)
                    if ((note.NotesMask & (1 << (v - 1))) != 0)
                        Board.GetCell(row, col).AddNote(v);
            }

            _undoService.Clear();
            _scoreService.SetScore(data.Score);
            _healthService.ChillModeActive = data.ChillModeActive;
            _healthService.SetHealth(data.Health);
            _timerService.SetElapsed(data.ElapsedSeconds);

            SetState(_playingState);
            return true;
        }

        public void Pause()
        {
            if (_currentState == _playingState)
                SetState(_pausedState);
        }

        public void Resume()
        {
            if (_currentState == _pausedState)
                SetState(_playingState);
        }

        public void ConfirmGameOver()
        {
            if (_currentState == _playingState)
                SetState(_gameOverState);
        }

        public void RestartCurrentGame()
        {
            if (Board == null) return;

            Board.ClearPlayerEntries();

            _undoService.Clear();
            _scoreService.Reset();
            _healthService.Reset();
            _timerService.Reset();
            _hintService.ResetForNewGame();

            _statsRepository.RegisterGameStarted(CurrentSize, CurrentDifficulty, IsExtraMode);

            SetState(_playingState);
        }

        public bool TryFillCell(int row, int col, int value)
        {
            if (Board == null || Board.IsFixed(row, col)) return false;

            var strategy = _generatorFactory.Get(CurrentSize);
            var command = new FillCellCommand(Board, row, col, value);
            _undoService.Execute(command);

            _notesService.CleanupPeerNotes(Board, row, col, value, strategy.BoxWidth, strategy.BoxHeight);

            bool correct = Board.IsCorrectValue(row, col, value);

            if (correct)
            {
                _scoreService.RegisterCorrectFill(CountFilledCells());
            }
            else
            {
                _healthService.RegisterMistake();
            }

            _signalBus.Fire(new CellFilledSignal { Row = row, Col = col, Value = value, IsCorrect = correct });

            return true;
        }

        public bool TryEraseCell(int row, int col)
        {
            if (Board == null || Board.IsFixed(row, col)) return false;

            var command = new EraseCellCommand(Board, row, col);
            _undoService.Execute(command);
            return true;
        }

        public bool TryToggleNote(int row, int col, int value)
        {
            if (Board == null || Board.IsFixed(row, col)) return false;
            if (Board.GetValue(row, col) != 0) return false;

            var command = new ToggleNoteCommand(Board, row, col, value);
            _undoService.Execute(command);
            return true;
        }

        public void Undo() => _undoService.UndoLast();

        public bool UseHint()
        {
            if (Board == null) return false;

            if (_hintService.TryUseHint(Board, out int row, out int col, out int value))
            {
                var strategy = _generatorFactory.Get(CurrentSize);

                Board.ClearNotes(row, col);
                _notesService.CleanupPeerNotes(Board, row, col, value, strategy.BoxWidth, strategy.BoxHeight);

                _signalBus.Fire(new CellFilledSignal { Row = row, Col = col, Value = value, IsCorrect = true });
                return true;
            }

            return false;
        }

        public void ReviveFromAd() => _healthService.Revive();
        public void GrantRewardedHint() => _hintService.GrantRewardedHint();

        public void SaveProgress()
        {
            if (Board == null) return;

            int n = Board.SideLength;
            var notes = new List<NoteEntry>();

            for (int r = 0; r < n; r++)
            {
                for (int c = 0; c < n; c++)
                {
                    var cell = Board.GetCell(r, c);
                    if (cell.Notes.Count == 0) continue;

                    ushort mask = 0;
                    foreach (var note in cell.Notes)
                        mask |= (ushort)(1 << (note - 1));

                    notes.Add(new NoteEntry { CellIndex = r * n + c, NotesMask = mask });
                }
            }

            var data = new GameSaveData
            {
                Size = CurrentSize,
                Difficulty = CurrentDifficulty,
                IsExtraMode = IsExtraMode,
                SolutionCells = Board.Solution,
                CurrentCells = Board.ExportCurrentValues(),
                IsFixedCell = Board.ExportFixedMask(),
                Notes = notes,
                Score = _scoreService.CurrentScore,
                Health = _healthService.CurrentHealth,
                ElapsedSeconds = _timerService.ElapsedSeconds,
                ChillModeActive = ChillModeActive,
                UndoHistory = new List<CommandSnapshot>() 
            };

            _saveService.SaveCurrentGame(data);
        }

        private void SetState(IGameState next)
        {
            _currentState?.Exit();
            _currentState = next;
            _currentState.Enter();

            if (next == _wonState) HandleWon();
            else if (next == _gameOverState) HandleGameOver();
        }

        private void HandleWon()
        {
            int finalScore = _scoreService.CurrentScore;

            if (!ChillModeActive)
            {
                int healthSpent = 3 - _healthService.CurrentHealth;
                _statsRepository.RegisterGameCompleted(
                    CurrentSize, CurrentDifficulty, IsExtraMode, finalScore, _timerService.ElapsedSeconds, healthSpent);

                _currencyService.Add(_currencyReward.GetReward(CurrentSize));
            }

            _saveService.ClearSave();
            _signalBus.Fire(new BoardCompletedSignal { FinalScore = finalScore });
        }

        private void HandleGameOver()
        {
            _saveService.ClearSave();
            _signalBus.Fire(new GameOverSignal());
        }

        private int CountFilledCells()
        {
            int n = Board.SideLength;
            int count = 0;

            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (Board.GetValue(r, c) != 0) count++;

            return count;
        }
    }
}
