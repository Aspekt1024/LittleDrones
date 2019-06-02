using Aspekt.UI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class InventoryUI : UIPanel
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                if (IsOpen)
                {
                    Close();
                }
                else
                {
                    Open();
                }
            }
        }
    }
}