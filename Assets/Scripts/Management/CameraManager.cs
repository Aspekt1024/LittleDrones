using UnityEngine;

namespace Aspekt.Drones
{
    public class CameraManager : IManager
    {
        public Camera Main { get; }

        public void Init()
        {
        }

        public CameraManager()
        {
            var cam = GameObject.FindGameObjectWithTag("MainCamera");
            if (cam == null)
            {
                Debug.LogError("No camera found with tag MainCamera");
                return;
            }

            Main = cam.GetComponent<Camera>();

            if (Main == null)
            {
                Debug.LogError("Object tagged MainCamera has no Camera component");
            }
        }
    }
}