using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    [Serializable]
    public abstract class AIGoal<L, V> : IAIGoal<L, V>
    {
        public float Priority { get; set; }

        private readonly Dictionary<L, V> conditions = new Dictionary<L, V>();

        protected IAIAgent<L, V> agent;

        public bool IsEnabled { get; private set; }

        public AIGoal(int priority)
        {
            Priority = priority;
        }

        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
            IsEnabled = true;
            SetConditions();
        }

        public Dictionary<L, V> GetConditions() => conditions;

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
            return GetType().Name;
        }

        public abstract void SetupGoal();
        public abstract void ResetGoal();

        protected abstract void SetConditions();

        protected void AddCondition(L label, V value)
        {
            if (conditions.ContainsKey(label))
            {
                conditions[label] = value;
            }
            else
            {
                conditions.Add(label, value);
            }
        }
    }
}