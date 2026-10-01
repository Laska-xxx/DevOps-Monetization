using Data;

namespace Signals
{
    public struct CellFilledSignal
    {
        public int Row;
        public int Col;
        public int Value;
        public bool IsCorrect;
    }

    public struct MistakeMadeSignal
    {
        public int RemainingHealth;
    }

    public struct HealthDepletedSignal { }

    public struct ComboChangedSignal
    {
        public int ComboCount;
    }

    public struct ScoreChangedSignal
    {
        public int NewScore;
        public int Delta;
    }

    public struct BoardCompletedSignal
    {
        public int FinalScore;
    }

    public struct HintUsedSignal
    {
        public bool WasFree;
    }

    public struct ReturnedToMainMenuSignal { }

    public struct GameOverSignal { }

    public struct ThemeChangedSignal
    {
        public ColorThemeSO Theme;
    }
}
