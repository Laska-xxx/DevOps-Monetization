using DG.Tweening;
using UnityEngine;

namespace Controllers
{
    public abstract class ScreenControllerBase : MonoBehaviour, IScreen
    {
        [SerializeField] protected CanvasGroup CanvasGroup;
        [SerializeField] private float _fadeDuration = 0.25f;

        public virtual void Show()
        {
            gameObject.SetActive(true);
            DOTween.Kill(this);
            CanvasGroup.alpha = 0f;
            CanvasGroup.DOFade(1f, _fadeDuration)
                .SetUpdate(true)
                .SetId(this)
                .OnStart(() => CanvasGroup.blocksRaycasts = true);
        }

        public virtual void Hide()
        {
            DOTween.Kill(this);
            CanvasGroup.DOFade(0f, _fadeDuration)
                .SetUpdate(true)
                .SetId(this)
                .OnComplete(() =>
                {
                    CanvasGroup.blocksRaycasts = false;
                    gameObject.SetActive(false);
                });
        }
    }
}
