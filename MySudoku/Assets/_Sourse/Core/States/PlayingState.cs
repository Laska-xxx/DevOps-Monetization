namespace Core.States
{
    public class PlayingState : IGameState
    {
        private readonly IGameStateHost _host;
        private readonly IGameState _wonState;

        public PlayingState(IGameStateHost host, IGameState wonState)
        {
            _host = host;
            _wonState = wonState;
        }

        public void Enter() { }
        public void Exit() { }

        public void Tick(float deltaTime)
        {
            _host.ElapsedSeconds += deltaTime;

            if (_host.IsBoardSolved)
            {
                _host.TransitionTo(_wonState);
            }
        }
    }
}
