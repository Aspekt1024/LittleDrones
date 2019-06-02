using UnityEngine;

namespace Aspekt.Drones
{
    /// <summary>
    /// The base class for all units in the game
    /// </summary>
    public abstract class UnitBase : MonoBehaviour, IUnit
    {
        private void Start()
        {
            GameManager.Units.RegisterUnit(this);
        }

        public void Remove()
        {
            GameManager.Units.UnregisterUnit(this);
            Destroy(gameObject);
        }

        public virtual string GetName()
        {
            return name;
        }
    }
}