using UnityEngine;

namespace Presentation.Views
{
    public class GameView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        public void Show()
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        public void Hide()
        {
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}
