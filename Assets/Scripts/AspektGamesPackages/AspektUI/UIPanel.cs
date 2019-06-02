using System.Collections;
using UnityEngine;

namespace Aspekt.UI
{
    public abstract class UIPanel : MonoBehaviour, IUIPanel
    {
#pragma warning disable 649
        [SerializeField] private bool visibleOnStartup;
        [SerializeField] private CanvasGroup canvasGroup;
#pragma warning restore 649

        private IUIAnimator uiAnimator;
        
        private enum States
        {
            Open, Opening, Closed, Closing
        }

        private States state;

        private Coroutine openRoutine;
        private Coroutine closeRoutine;

        public bool IsOpen => state == States.Open || state == States.Opening;
        public bool IsClosed => state == States.Closed || state == States.Closing;

        public void Init()
        {
            uiAnimator = CreateAnimator();
            
            if (visibleOnStartup)
            {
                OpenImmediate();
            }
            else
            {
                CloseImmediate();
            }
        }
        
        public void Open()
        {
            if (IsOpen) return;
            if (openRoutine != null) StopCoroutine(openRoutine);
            openRoutine = StartCoroutine(OpenRoutine());
        }

        public void Close()
        {
            if (IsClosed) return;
            if (closeRoutine != null) StopCoroutine(closeRoutine);
            closeRoutine = StartCoroutine(CloseRoutine());
        }

        public IEnumerator OpenRoutine()
        {
            if (IsOpen) yield break;
            state = States.Opening;
            
            if (openRoutine != null) StopCoroutine(openRoutine);
            yield return StartCoroutine(uiAnimator.AnimateIn());;
            
            OpenImmediate();
        }

        public IEnumerator CloseRoutine()
        {
            if (IsClosed) yield break;
            state = States.Closing;
            
            if (closeRoutine != null) StopCoroutine(closeRoutine);
            yield return StartCoroutine(uiAnimator.AnimateOut());
            
            CloseImmediate();
        }

        public void OpenImmediate()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            state = States.Open;
        }

        public void CloseImmediate()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            state = States.Closed;
        }

        protected virtual IUIAnimator CreateAnimator()
        {
            return new UIFadeAnimator(canvasGroup);
        }
    }
}