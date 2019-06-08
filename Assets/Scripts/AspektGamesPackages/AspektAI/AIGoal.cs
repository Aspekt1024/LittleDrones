using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    [Serializable]
    public abstract class AIGoal<L, V> : IAIGoal<L, V>
    {
#pragma warning disable 649
        [SerializeField] private float priority;
#pragma warning restore 649
        
        public float Priority
        {
            get => priority;
            private set => priority = value;
        }

        private readonly Dictionary<L, V> conditions = new Dictionary<L, V>();

        protected IAIAgent<L, V> agent;

        public bool IsEnabled { get; private set; }

        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
            IsEnabled = true;
            SetConditions();
        }

        public Dictionary<L, V> GetConditions() => conditions;

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

        public abstract void ResetGoal();

        protected abstract void SetConditions();

        protected void AddCondition(L label, V value)
        {
            conditions.Add(label, value);
        }
    }
}