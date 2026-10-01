namespace Controllers
{
    public interface IScreenService
    {
        void Show<T>() where T : IScreen;
        void Hide<T>() where T : IScreen;
        void ShowExclusive<T>() where T : IScreen;
    }
}
