namespace Services
{
    public interface ITimerService
    {
        float ElapsedSeconds { get; }
        void Tick(float deltaTime);
        void Reset(float startingSeconds = 0f);
        void SetElapsed(float seconds);
    }

    public class TimerService : ITimerService
    {
        public float ElapsedSeconds { get; private set; }

        public void Tick(float deltaTime) => ElapsedSeconds += deltaTime;

        public void Reset(float startingSeconds = 0f) => ElapsedSeconds = startingSeconds;

        public void SetElapsed(float seconds) => ElapsedSeconds = seconds;
    }
}
