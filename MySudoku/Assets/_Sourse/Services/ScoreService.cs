using UnityEngine;
using Zenject;
using Data;
using Signals;

namespace Services
{
    public interface IScoreService
    {
        int CurrentScore { get; }
        void RegisterCorrectFill(int filledCellsCount);
        void SetScore(int score);
        void Reset();
    }

    public class ScoreService : IScoreService, IInitializable, System.IDisposable
    {
        private readonly SignalBus _signalBus;
        private readonly ScoreSettingsSO _settings;

        private int _score;
        private int _combo;
        private float _lastFillTimestamp;

        public int CurrentScore => _score;

        public ScoreService(SignalBus signalBus, ScoreSettingsSO settings)
        {
            _signalBus = signalBus;
            _settings = settings;
        }

        public void Initialize()
        {
            _signalBus.Subscribe<MistakeMadeSignal>(OnMistake);
            _lastFillTimestamp = Time.time;
        }

        public void Dispose()
        {
            _signalBus.Unsubscribe<MistakeMadeSignal>(OnMistake);
        }

        public void RegisterCorrectFill(int filledCellsCount)
        {
            float delta = Time.time - _lastFillTimestamp;
            _lastFillTimestamp = Time.time;

            _combo++;

            int gained = _settings.BaseScorePerCell;
            gained += _combo * _settings.ComboBonusPerStep;
            gained += filledCellsCount / 5; 

            if (delta <= _settings.FastFillThresholdSeconds)
                gained += _settings.FastFillBonus;

            _score += gained;

            _signalBus.Fire(new ScoreChangedSignal { NewScore = _score, Delta = gained });
            _signalBus.Fire(new ComboChangedSignal { ComboCount = _combo });
        }

        public void SetScore(int score)
        {
            _score = score;
            _signalBus.Fire(new ScoreChangedSignal { NewScore = _score, Delta = 0 });
        }

        public void Reset()
        {
            _score = 0;
            _combo = 0;
            _lastFillTimestamp = Time.time;
        }

        private void OnMistake(MistakeMadeSignal signal)
        {
            _combo = 0;
            _signalBus.Fire(new ComboChangedSignal { ComboCount = 0 });
        }
    }
}
