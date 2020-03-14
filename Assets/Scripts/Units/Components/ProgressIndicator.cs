using Aspekt.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Aspekt.Drones
{
    public class ProgressIndicator : UIPanel
    {
        #pragma warning disable 649
        [SerializeField] private Slider slider;
        #pragma warning restore 649
        
        private Transform tf;
        private Animator anim;
        private Transform target;
        private static readonly int IsComplete = Animator.StringToHash("isComplete");

        private void Awake()
        {
            tf = transform;
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (IsOpen)
            {
                FollowTarget(); 
            }
        }

        public void SetValue(float value, Transform tar)
        {
            anim.SetBool(IsComplete, false);
            target = tar;
            slider.value = value;
        }

        public void Blink()
        {
            anim.SetBool(IsComplete, true);
        }

        private void FollowTarget()
        {
            if (target == null)
            {
                CloseImmediate();
                return;
            }

            var pos = GameManager.Camera.Main.WorldToScreenPoint(target.position);
            pos += new Vector3(0f, 20f, 0f);
            tf.position = pos;
        }
        
        
    }
}