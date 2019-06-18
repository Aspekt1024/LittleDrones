using System;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// Scans the given radius for all objects within the given layermask
    /// </summary>
    [Serializable]
    public abstract class LayerSensorBase : Sensor<AIAttributes, object>
    {
        [Range(1f, 200f)] public float detectionRadius = 100f;
        
        /// <summary>
        /// The minimum amount of time that must pass before scanning can occur again
        /// A call to <see cref="Scan"/> prior to this will return the previous scan's results
        /// </summary>
        public float maxRefreshRate = 0.5f;
        
        protected abstract LayerMask ObjectLayerMask { get; set; }

        private Collider[] scannedObjects;
        private float timeLastScanned;
        private bool cacheCleared = true;

        public bool IsObjectObtainable<T>(Predicate<T> predicate) where T : MonoBehaviour
        {
            Scan();
            if (scannedObjects == null) return false;

            return scannedObjects
                .Select(FindBaseComponent<T>)
                .Where(c => c != null)
                .Any(c => predicate(c));
        }   
        
        public T GetClosest<T>(Predicate<T> predicate) where T : MonoBehaviour
        {
            Scan();

            float dist = float.MaxValue;
            T closest = null;
            foreach (var obj in scannedObjects)
            {
                if (!obj.gameObject.activeSelf) continue;
                var d = Vector3.Distance(Agent.Transform.position, obj.transform.position);
                if (d >= dist) continue;

                var candidate = FindBaseComponent<T>(obj);
                if (candidate == null) continue;
                if (!predicate(candidate)) continue;

                closest = candidate;
                dist = d;
            }
            
            return closest;
        }
        
        private void Scan()
        {
            if (Time.time < timeLastScanned + maxRefreshRate) return;
            timeLastScanned = Time.time;
            cacheCleared = false;
            
            var colliders = Physics.OverlapSphere(Agent.Transform.position, detectionRadius, ObjectLayerMask);
            scannedObjects = colliders.ToArray();
            Agent.LogInfo(this, $"scanned {scannedObjects.Length} objects");
        }

        protected override void OnTick(float deltaTime)
        {
            if (cacheCleared || Time.time < timeLastScanned + maxRefreshRate + 1f) return;
            cacheCleared = true;
            scannedObjects = null;
        }

        private static T FindBaseComponent<T>(Collider c)
        {
            var baseComponent = c.GetComponentInParent<T>();
            if (baseComponent == null)
            {
                baseComponent = c.GetComponent<T>();
            }
            return baseComponent;
        }
    }
}