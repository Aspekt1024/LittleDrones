using System.Collections.Generic;
using System.Linq;

namespace Aspekt.Drones
{
    public class AbilityManager : IAbilityManager
    {
        private readonly List<IAbility> abilities = new List<IAbility>();

        public bool AddAbility(IAbility ability)
        {
            if (abilities.Contains(ability)) return false;
            abilities.Add(ability);
            return true;
        }

        public T GetAbility<T>() where T : IAbility
        {
            foreach (var ability in abilities)
            {
                if (ability is T a) return a;
            }
            return default;
        }

        public bool HasAbility<T>() where T : IAbility
        {
            return abilities.Any(a => a is T);
        }
    }
}