using UnityEngine;

namespace Aspekt.Drones
{
    public enum Layers
    {
        Resource,
        FixedObject,
        Obstacle,
        Terrain,
        Building,
    }
    
    public static class LayerUtil
    {
        public static LayerMask GetMask(params Layers[] layers)
        {
            var mask = 0;
            foreach (var layer in layers)
            {
                mask |= 1 << LayerMask.NameToLayer(layer.ToString());
            }
            return mask;
        }
    }
}