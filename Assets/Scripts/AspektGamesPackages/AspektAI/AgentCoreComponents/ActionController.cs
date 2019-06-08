using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.AI
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

        public void AddAction<TAction>() where TAction : IAIAction<L, V>, new()
        {
            if (actions.Any(s => s is L)) return;
            
            var action = new TAction();
            action.Init(agent);
            actions.Add(action);
        }

        public void RemoveAction<TAction>() where TAction : IAIAction<L, V>
        {
            actions.RemoveAll(s => s is TAction);
        }
    }
}