using System;
using System.Collections.Generic;
using Zenject;

using Core.Board;
using Core.Generation;
using Presentation.Views;
using Services;
using Signals;

namespace Controllers
{
    public class GameController : IInitializable, IDisposable, ITickable
    {
        private readonly BoardView _boardView;
        private readonly HudView _hudView;
        private readonly NumberPadView _numberPadView;
        private readonly GameView _gameView;
        private readonly ConfirmationDialogView _confirmationDialog;
        private readonly PauseOverlayView _pauseOverlay;
        private readonly PurchaseOfferController _purchaseOffer;
        private readonly GameSessionController _session;
        private readonly INotesService _notesService;
        private readonly IUndoService _undoService;
        private readonly SignalBus _signalBus;
        private readonly IBoardGeneratorFactory _generatorFactory;

        private CellView _selectedCell;
        private int? _pendingValue;

        public GameController(
            BoardView boardView,
            HudView hudView,
            NumberPadView numberPadView,
            GameView gameView,
            ConfirmationDialogView confirmationDialog,
            PauseOverlayView pauseOverlay,
            PurchaseOfferController purchaseOffer,
            GameSessionController session,
            INotesService notesService,
            IUndoService undoService,
            SignalBus signalBus,
            IBoardGeneratorFactory generatorFactory)
        {
            _boardView = boardView;
            _hudView = hudView;
            _numberPadView = numberPadView;
            _gameView = gameView;
            _confirmationDialog = confirmationDialog;
            _pauseOverlay = pauseOverlay;
            _purchaseOffer = purchaseOffer;
            _session = session;
            _notesService = notesService;
            _undoService = undoService;
            _signalBus = signalBus;
            _generatorFactory = generatorFactory;
        }

        public void Initialize()
        {
            _boardView.CellClicked += OnCellClicked;
            _numberPadView.NumberClicked += OnNumberClicked;
            _numberPadView.EraseClicked += OnEraseClicked;
            _numberPadView.NotesToggleClicked += OnNotesToggleClicked;
            _numberPadView.HintClicked += OnHintClicked;
            _numberPadView.UndoClicked += OnUndoClicked;

            _pauseOverlay.ResumeClicked += OnResumeClicked;
            _pauseOverlay.ExitToMenuClicked += OnPauseExitClicked;

            _hudView.ExitClicked += OnHudExitClicked;

            _signalBus.Subscribe<CellFilledSignal>(OnCellFilledSignal);
            _signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
            _signalBus.Subscribe<MistakeMadeSignal>(OnMistakeMade);
            _signalBus.Subscribe<ComboChangedSignal>(OnComboChanged);
            _signalBus.Subscribe<BoardCompletedSignal>(OnBoardCompleted);
        }

        public void Dispose()
        {
            _boardView.CellClicked -= OnCellClicked;
            _numberPadView.NumberClicked -= OnNumberClicked;
            _numberPadView.EraseClicked -= OnEraseClicked;
            _numberPadView.NotesToggleClicked -= OnNotesToggleClicked;
            _numberPadView.HintClicked -= OnHintClicked;
            _numberPadView.UndoClicked -= OnUndoClicked;

            _pauseOverlay.ResumeClicked -= OnResumeClicked;
            _pauseOverlay.ExitToMenuClicked -= OnPauseExitClicked;

            _hudView.ExitClicked -= OnHudExitClicked;

            _signalBus.Unsubscribe<CellFilledSignal>(OnCellFilledSignal);
            _signalBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);
            _signalBus.Unsubscribe<MistakeMadeSignal>(OnMistakeMade);
            _signalBus.Unsubscribe<ComboChangedSignal>(OnComboChanged);
            _signalBus.Unsubscribe<BoardCompletedSignal>(OnBoardCompleted);
        }

        public void Tick()
        {
            if (_session.Board == null) return;
            _hudView.SetTime(_session.ElapsedSeconds);
        }

        public void ActivateGameScreen()
        {
            var board = _session.Board;
            var strategy = _generatorFactory.Get(_session.CurrentSize);

            _boardView.BuildBoard(board.SideLength, strategy.BoxWidth, strategy.BoxHeight);
            _numberPadView.BuildForSize(board.SideLength);
            _numberPadView.SetUndoAvailable(false);
            _numberPadView.SetHintAvailable(true);

            RenderFullBoard(board);
            RefreshNumberPadAvailability();

            _hudView.SetScore(_session.CurrentScore);
            _hudView.SetHealth(_session.CurrentHealth);
            _hudView.SetTime(_session.ElapsedSeconds);
            _hudView.SetDifficultyLabel(BuildDifficultyLabel());

            _selectedCell = null;
            _pendingValue = null;
            _boardView.ClearHighlights();

            _gameView.Show();
        }

        public void RefreshAfterRestart()
        {
            var board = _session.Board;
            if (board == null) return;

            RenderFullBoard(board);
            RefreshNumberPadAvailability();
            _numberPadView.SetUndoAvailable(false);
            _numberPadView.SetHintAvailable(true);

            _hudView.SetScore(_session.CurrentScore);
            _hudView.SetHealth(_session.CurrentHealth);
            _hudView.SetTime(_session.ElapsedSeconds);

            _selectedCell = null;
            _pendingValue = null;
            _boardView.ClearHighlights();
        }

        private string BuildDifficultyLabel() =>
            _session.IsExtraMode
                ? $"{(int)_session.CurrentSize}x{(int)_session.CurrentSize} / {_session.CurrentDifficulty}"
                : _session.CurrentDifficulty.ToString();

        private void RenderFullBoard(BoardModel board)
        {
            int n = board.SideLength;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                {
                    int value = board.GetValue(r, c);
                    _boardView.RenderCell(r, c, value, board.IsFixed(r, c), board.GetCell(r, c).Notes, IsCellWrong(board, r, c, value));
                }
        }

        private bool IsCellWrong(BoardModel board, int row, int col, int value) =>
            value != 0 && !board.IsFixed(row, col) && !board.IsCorrectValue(row, col, value);

        private void OnCellClicked(int row, int col)
        {
            var board = _session.Board;
            if (board == null) return;

            _selectedCell = _boardView.GetCell(row, col);

            var strategy = _generatorFactory.Get(_session.CurrentSize);
            _boardView.HighlightPeers(row, col, strategy.BoxWidth, strategy.BoxHeight);

            RefreshSameValueHighlight();
        }

        private void RefreshSameValueHighlight()
        {
            if (_selectedCell == null) return;

            var board = _session.Board;
            if (board == null) return;

            _boardView.ClearSameValueHighlights();

            int value = board.GetValue(_selectedCell.Row, _selectedCell.Col);

            if (value == 0)
            {
                _boardView.SetSameValueHighlight(_selectedCell.Row, _selectedCell.Col, true);
                return;
            }

            int n = board.SideLength;
            for (int r = 0; r < n; r++)
                for (int c = 0; c < n; c++)
                    if (board.GetValue(r, c) == value)
                        _boardView.SetSameValueHighlight(r, c, true);
        }

        private void RefreshNumberPadAvailability()
        {
            var board = _session.Board;
            if (board == null) return;

            for (int value = 1; value <= board.SideLength; value++)
                _numberPadView.SetNumberAvailable(value, !board.IsValueComplete(value));
        }

        private void OnNumberClicked(int value)
        {
            if (_selectedCell == null) return;

            if (_notesService.NotesModeActive)
            {
                _session.TryToggleNote(_selectedCell.Row, _selectedCell.Col, value);
                RefreshCellNotes(_selectedCell.Row, _selectedCell.Col);
                return;
            }

            _pendingValue = value;
            _confirmationDialog.ShowWithMessage(
                $"Поставить {value} в выбранную клетку?",
                onConfirm: ApplyPendingValue,
                onCancel: CancelPendingValue);
        }

        private void ApplyPendingValue()
        {
            if (_selectedCell == null || _pendingValue == null) return;

            _session.TryFillCell(_selectedCell.Row, _selectedCell.Col, _pendingValue.Value);
            _pendingValue = null;
        }

        private void CancelPendingValue() => _pendingValue = null;

        private void OnEraseClicked()
        {
            if (_selectedCell == null) return;
            if (!_session.TryEraseCell(_selectedCell.Row, _selectedCell.Col)) return;

            _boardView.RenderCell(_selectedCell.Row, _selectedCell.Col, 0, false, Array.Empty<int>(), isWrong: false);
            RefreshSameValueHighlight();
            RefreshNumberPadAvailability();
        }

        private void OnNotesToggleClicked()
        {
            _notesService.ToggleNotesMode();
            _numberPadView.SetNotesModeVisual(_notesService.NotesModeActive);
        }

        private void OnHintClicked()
        {
            if (!_session.HasFreeHint)
            {
                _purchaseOffer.RequestHintPurchase();
                return;
            }

            _confirmationDialog.ShowWithMessage(
                "Использовать бесплатную подсказку?",
                onConfirm: () =>
                {
                    if (_session.UseHint())
                        _numberPadView.SetHintAvailable(false);
                });
        }

        private void OnUndoClicked()
        {
            _session.Undo();
            RenderFullBoard(_session.Board);
            _numberPadView.SetUndoAvailable(_undoService.CanUndo);
            RefreshSameValueHighlight();
            RefreshNumberPadAvailability();
        }

        public void RequestPause()
        {
            _session.Pause();
            _pauseOverlay.Show();
        }

        private void OnResumeClicked()
        {
            _pauseOverlay.Hide();
            _session.Resume();
        }

        private void OnPauseExitClicked()
        {
            _pauseOverlay.Hide();
            ExitToMenuWithSave();
        }

        private void OnHudExitClicked() => ExitToMenuWithSave();

        private void ExitToMenuWithSave()
        {
            _session.SaveProgress();
            ReturnToMainMenu();
        }

        public void ReturnToMainMenu()
        {
            _gameView.Hide();
            _signalBus.Fire(new ReturnedToMainMenuSignal());
        }

        private void RefreshCellNotes(int row, int col)
        {
            var board = _session.Board;
            var cell = board.GetCell(row, col);
            _boardView.RenderCell(row, col, cell.Value, cell.IsFixed, cell.Notes, IsCellWrong(board, row, col, cell.Value));
        }

        private void OnCellFilledSignal(CellFilledSignal signal)
        {
            RenderFullBoard(_session.Board);

            var cellView = _boardView.GetCell(signal.Row, signal.Col);

            if (signal.IsCorrect)
                cellView.PlayCorrectFillAnimation();
            else
                cellView.PlayMistakeAnimation();

            _numberPadView.SetUndoAvailable(_undoService.CanUndo);
            RefreshSameValueHighlight();
            RefreshNumberPadAvailability();
        }

        private void OnScoreChanged(ScoreChangedSignal signal) => _hudView.SetScore(signal.NewScore);

        private void OnMistakeMade(MistakeMadeSignal signal) => _hudView.SetHealth(signal.RemainingHealth);

        private void OnComboChanged(ComboChangedSignal signal)
        {
            if (signal.ComboCount > 1) _hudView.PlayComboPulse();
        }

        private void OnBoardCompleted(BoardCompletedSignal signal)
        {
            _boardView.PlayWinSequence();
        }
    }
}
