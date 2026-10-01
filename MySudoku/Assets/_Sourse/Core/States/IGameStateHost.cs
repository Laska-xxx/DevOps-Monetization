namespace Core.States
{
    public interface IGameStateHost
    {
        void TransitionTo(IGameState next);
        float ElapsedSeconds { get; set; }
        bool IsBoardSolved { get; }
    }
}
