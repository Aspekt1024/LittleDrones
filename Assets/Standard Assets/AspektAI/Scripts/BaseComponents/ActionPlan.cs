using System.Collections.Generic;

namespace Aspekt.AI
{
    /// <summary>
    /// An action plan fulfils a Goal with a set of Actions
    /// </summary>
    public struct ActionPlan<L, V>
    {
        public IAIGoal<L, V> Goal;
        public Queue<IAIAction<L, V>> Actions;

        public bool IsValid => Goal != null;
    }
}