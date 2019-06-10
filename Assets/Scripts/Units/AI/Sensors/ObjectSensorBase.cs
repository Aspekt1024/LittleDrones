using System;
using System.Collections.Generic;
using System.Linq;
using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    [Serializable]
    public abstract class ObjectSensorBase<T> : Sensor<AIAttributes, object> where T : MonoBehaviour
    {
        [Range(1f, 200f)] public float detectionRadius = 100f;
        
        /// <summary>
        /// The minimum amount of time that must pass before scanning can occur again
        /// A call to <see cref="Scan"/> prior to this will return the previous scan's results
        /// </summary>
        public float maxRefreshRate = 0.5f;

        protected abstract LayerMask ObjectLayerMask { get; set; }

        private T[] scannedObjects;
        private float timeLastSensed;

        protected T[] Scan(Predicate<T> predicate)
        {
            if (Time.time < timeLastSensed + maxRefreshRate) return scannedObjects;
            timeLastSensed = Time.time;
            
            var colliders = Physics.OverlapSphere(Agent.Transform.position, detectionRadius, ObjectLayerMask);
            scannedObjects = colliders.Select(FindBaseComponent).Where(c => predicate(c)).ToArray();
            Agent.LogInfo(this, $"found {scannedObjects.Length} {typeof(T).Name}s");
            return scannedObjects;
        }
        
        protected T GetClosestObject(Vector3 pos, Predicate<T> predicate)
        {
            Scan(predicate);

            float dist = float.MaxValue;
            T closest = null;
            foreach (var obj in scannedObjects)
            {
                if (!obj.gameObject.activeSelf) continue;
                var d = Vector3.Distance(pos, obj.transform.position);
                if (d >= dist) continue;
                
                closest = obj;
                dist = d;
            }

            return closest;
        }

        private static T FindBaseComponent(Collider c)
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