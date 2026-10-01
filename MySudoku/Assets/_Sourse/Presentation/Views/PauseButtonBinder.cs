using UnityEngine;
using UnityEngine.UI;
using Zenject;

using Controllers;

namespace Presentation.Views
{
    public class PauseButtonBinder : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;

        private GameController _gameController;

        [Inject] private void Init(GameController gameController) => _gameController = gameController;

        private void Awake() => _pauseButton.onClick.AddListener(() => _gameController.RequestPause());
    }
}
