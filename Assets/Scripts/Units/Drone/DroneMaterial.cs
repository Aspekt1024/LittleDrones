using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.Drones
{
    public class DroneMaterial
    {
        private readonly List<Material> materials = new List<Material>();

        private float xOffset;
        private float yOffset;

        private const float ScrollSpeed = 0.5f;
        
        public DroneMaterial(List<MeshRenderer> renderers)
        {
            foreach (var renderer in renderers)
            {
                materials.Add(renderer.material);
            }
        }

        public void Tick()
        {
            xOffset += Time.deltaTime * ScrollSpeed;
            yOffset += Time.deltaTime * ScrollSpeed / 2f;

            foreach (var material in materials)
            {
                material.SetTextureOffset("_MainTex", new Vector2(xOffset, yOffset));
                material.SetTextureOffset("_Normal", new Vector2(xOffset, yOffset));
            }
        }
    }
}