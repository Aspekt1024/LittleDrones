using System.Collections.Generic;

namespace Aspekt.AI
{
    public abstract class AIGoal<L, V> : IAIGoal<L, V>
    {
        public float Priority { get; private set; } = 1f;

        private readonly Dictionary<L, V> conditions = new Dictionary<L, V>();

        public bool IsEnabled { get; private set; }
        
        public void Init()
        {
            IsEnabled = true;
            SetConditions();
        }
        
        public Dictionary<L, V> GetConditions()
        {
            return conditions;
        }

        public void SetPriority(float priority) => Priority = priority;
        
        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public override string ToString()
        {
            return GetType().ToString();
        }

        /// <summary>
        /// Used to set the conditions using AddCondition(T, R)
        /// </summary>
        protected abstract void SetConditions();

        protected void AddCondition(L label, V value)
        {
            conditions.Add(label, value);
        }
    }
}