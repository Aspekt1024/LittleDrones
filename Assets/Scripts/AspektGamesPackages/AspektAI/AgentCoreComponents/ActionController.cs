using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.AI.Internal
{
    public class ActionController<L, V> : IActionController<L, V>
    {
        private IAIAgent<L, V> agent;
        
        private readonly List<IAIAction<L, V>> actions = new List<IAIAction<L, V>>();
        
        public void Init(IAIAgent<L, V> agent)
        {
            this.agent = agent;
        }
        
        public List<IAIAction<L, V>> GetActions()
        {
            return actions;
        }

        public void DisableActions()
        {
            foreach (var action in actions)
            {
                action.Disable();
            }
        }

        public void EnableActions()
        {
            foreach (var action in actions)
            {
                action.Enable();
            }
        }

        public void AddAction(IAIAction<L, V> action)
        {
            var matchedActions = actions.Where(g => g is L).ToArray();
            foreach (var matchedAction in matchedActions)
            {
                RemoveAction(matchedAction);
            }
            action.Init(agent);
            actions.Add(action);
        }

        public void RemoveAction(IAIAction<L, V> action)
        {
            action.OnRemove();
            actions.Remove(action);
        }
    }
}