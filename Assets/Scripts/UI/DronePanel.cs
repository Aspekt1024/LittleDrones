using Aspekt.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Aspekt.Drones
{
    public class DronePanel : UIPanel, IPointerClickHandler
    {
        public Vector2 openedPos;
        public Vector2 closedPos;
        
#pragma warning disable 649
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private DroneAIAgent aiAgent;
#pragma warning restore 649
        
        protected override IUIAnimator CreateAnimator()
        { 
            return new UISlideAnimator(closedPos, openedPos, GetComponent<RectTransform>());
        }

        public void ToggleButtonClicked()
        {
            if (IsClosed)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsClosed)
            {
                Open();
            }
        }

        private void Awake()
        {
            nameText.text = aiAgent.GetComponentInParent<Drone>().GetName();
        }

        private void Update()
        {
            statusText.text = aiAgent.GetStatus();
        }
    }
}