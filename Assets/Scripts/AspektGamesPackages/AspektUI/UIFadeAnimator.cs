using System.Collections;
using UnityEngine;

namespace Aspekt.UI
{
    public class UIFadeAnimator : IUIAnimator
    {
        private readonly CanvasGroup canvasGroup;
        private const float FadeTime = 0.1f;
        
        public UIFadeAnimator(CanvasGroup canvasGroup)
        {
            this.canvasGroup = canvasGroup;
        }
        
        public IEnumerator AnimateIn()
        {
            float animStartTime = Time.unscaledTime;
            while (Time.unscaledTime < animStartTime + FadeTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, (Time.unscaledTime - animStartTime) / FadeTime);;
                yield return null;
            }
        }

        public IEnumerator AnimateOut()
        {
            float animStartTime = Time.unscaledTime;
            while (Time.unscaledTime < animStartTime + FadeTime)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, (Time.unscaledTime - animStartTime) / FadeTime);;
                yield return null;
            }
        }
    }
}