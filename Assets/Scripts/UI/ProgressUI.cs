using System.Collections;
using Aspekt.UI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class ProgressUI : UIPanel
    {
        #pragma warning disable 649
        [SerializeField] private ProgressIndicator progressIndicator;
        #pragma warning restore 649

        private Coroutine showCompleteRoutine;
        
        public void ShowProgress(float progress, Transform obj)
        {
            progressIndicator.SetValue(progress, obj);
            progressIndicator.Open();
        }

        public void ShowProgressComplete(Transform obj)
        {
            progressIndicator.SetValue(1f, obj);
            progressIndicator.Open();
            if (showCompleteRoutine != null) StopCoroutine(showCompleteRoutine);
            showCompleteRoutine = StartCoroutine(ShowCompleteRoutine());
        }

        public void StopProgress(Transform obj)
        {
            progressIndicator.Close();
        }

        private IEnumerator ShowCompleteRoutine()
        {
            progressIndicator.Blink();
            yield return new WaitForSeconds(1f);
            progressIndicator.Close();
        }
    }
}