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

        private bool blocksRaycasts;
        private bool interactable;

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
            blocksRaycasts = canvasGroup.blocksRaycasts;
            interactable = canvasGroup.interactable;
            
            if (visibleOnStartup)
            {
                OpenImmediate();
            }
            else
            {
                CloseImmediate();
            }
        }

        public void Open(float delay = 0f)
        {
            if (IsOpen) return;
            StopRoutines();
            openRoutine = StartCoroutine(OpenRoutine(delay));
        }

        public void Close(float delay = 0f)
        {
            if (IsClosed) return;
            StopRoutines();
            closeRoutine = StartCoroutine(CloseRoutine(delay));
        }

        public IEnumerator OpenRoutine(float delay = 0f)
        {
            if (IsOpen) yield break;
            state = States.Opening;
            
            yield return StartCoroutine(uiAnimator.AnimateIn(delay));;
            
            OpenImmediate();
        }

        public IEnumerator CloseRoutine(float delay = 0f)
        {
            if (IsClosed) yield break;
            state = States.Closing;
            
            yield return StartCoroutine(uiAnimator.AnimateOut(delay));
            
            CloseImmediate();
        }

        public void OpenImmediate()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = blocksRaycasts;
            canvasGroup.interactable = interactable;
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

        private void StopRoutines()
        {
            if (openRoutine != null) StopCoroutine(openRoutine);
            if (closeRoutine != null) StopCoroutine(closeRoutine);
        }
    }
}