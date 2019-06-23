using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public abstract class DroneAction : AIAction<AIAttributes, object>
    {
        private IAbilityManager abilities; 
        
        public void OnAttach(IUnit unit)
        {
            abilities = unit.Abilities;
            if (abilities == null)
            {
                Debug.LogWarning($"Action attached to unit with null {nameof(IAbilityManager)}");
            }
        }

        protected T GetAbility<T>() where T : IAbility => abilities.GetAbility<T>();
    }
}