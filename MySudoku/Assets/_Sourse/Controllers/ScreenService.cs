using System;
using System.Collections.Generic;
using Zenject;

namespace Controllers
{
    public class ScreenService : IScreenService
    {
        private readonly DiContainer _container;
        private readonly List<Type> _screenTypes;

        public ScreenService(DiContainer container, List<Type> screenTypes)
        {
            _container = container;
            _screenTypes = screenTypes;
        }

        public void Show<T>() where T : IScreen => Resolve<T>()?.Show();

        public void Hide<T>() where T : IScreen => Resolve<T>()?.Hide();

        public void ShowExclusive<T>() where T : IScreen
        {
            foreach (var type in _screenTypes)
            {
                var screen = (IScreen)_container.Resolve(type);
                if (type == typeof(T)) screen.Show();
                else screen.Hide();
            }
        }

        private T Resolve<T>() where T : IScreen
        {
            foreach (var type in _screenTypes)
                if (type == typeof(T))
                    return (T)_container.Resolve(type);
            return default;
        }
    }
}
