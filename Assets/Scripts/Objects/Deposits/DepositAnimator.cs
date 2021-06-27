using System;
using UnityEngine;

namespace Aspekt.Drones
{
    public class DepositAnimator
    {
        
        [Serializable]
        public class Settings
        {
            public bool enabled;
            public Vector2 pulseSize;
            public float rotationSpeed;
            public MeshRenderer meshRenderer;
            public float loopDuration;
        }

        private readonly Settings settings;
        private readonly Material material;

        private float loopTime;
        
        public DepositAnimator(Settings settings)
        {
            this.settings = settings;
            material = settings.meshRenderer.material;
        }

        public void Tick()
        {
            if (!settings.enabled) return;
            
            loopTime = (loopTime + Time.deltaTime) % (settings.loopDuration);
            var loopRatio = loopTime / settings.loopDuration;
            var loopValue = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * loopRatio));
            
            material.SetFloat("_TexScale", Mathf.Lerp(settings.pulseSize.x, settings.pulseSize.y, loopValue));
            
            settings.meshRenderer.transform.Rotate(Vector3.up, settings.rotationSpeed * Time.deltaTime);
        }
        
    }
}