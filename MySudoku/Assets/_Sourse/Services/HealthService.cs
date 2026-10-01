using Zenject;
using Signals;

namespace Services
{
    public interface IHealthService
    {
        int CurrentHealth { get; }
        bool ChillModeActive { get; set; }

        void Reset(int startingHealth = 3);
        void SetHealth(int health);
        void RegisterMistake();
        void Revive();
    }

    public class HealthService : IHealthService
    {
        private const int StartingHealth = 3;

        private readonly SignalBus _signalBus;

        private int _health;

        public int CurrentHealth => _health;
        public bool ChillModeActive { get; set; }

        public HealthService(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Reset(int startingHealth = StartingHealth) => _health = startingHealth;

        public void SetHealth(int health) => _health = health;

        public void RegisterMistake()
        {
            if (ChillModeActive) return;
            if (_health <= 0) return;

            _health--;
            _signalBus.Fire(new MistakeMadeSignal { RemainingHealth = _health });

            if (_health <= 0)
                _signalBus.Fire(new HealthDepletedSignal());
        }

        public void Revive()
        {
            if (_health <= 0) 
                _health = 1;
        }
    }
}
