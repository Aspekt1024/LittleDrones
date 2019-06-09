using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Aspekt.Drones
{
    public class PowerButton : MonoBehaviour, IPointerClickHandler
    {
#pragma warning disable 649
        [SerializeField] private Color offColor;
        [SerializeField] private Color onColor;
        [SerializeField] private Drone drone;
#pragma warning restore 649

        private Image buttonImage;

        private enum States
        {
            Off, On
        }

        private States state;

        private void Awake()
        {
            state = States.Off;
            buttonImage = GetComponent<Image>();
            buttonImage.color = offColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (state == States.On)
            {
                state = States.Off;
                drone.PowerOff();
                buttonImage.color = offColor;
            }
            else
            {
                state = States.On;
                drone.PowerOn();
                buttonImage.color = onColor;
            }
        }
    }
}