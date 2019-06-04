using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Aspekt.AI.Core
{
    public class ActionController<T, R> : IActionController<T, R>
    {
        private IAIAgent<T, R> agent;
        private IMemory<T, R> memory;
        
        private enum States
        {
            NotInitialised, NotInitialisedAlerted,
            Enabled, Disabled
        }
        private States state = States.NotInitialised;
        
        private List<IAIAction<T, R>> actions = new List<IAIAction<T, R>>();
        
        public void Init(IAIAgent<T, R> agent, IMemory<T, R> memory)
        {
            this.agent = agent;
            this.memory = memory;

            state = States.Enabled;

            if (agent is MonoBehaviour mb)
            {
                actions = mb.GetComponentsInChildren<IAIAction<T, R>>().ToList();
            }
            else
            {
                Debug.LogError("AI Agents must inherit from MonoBehaviour.");
            }
            
            foreach (var action in actions)
            {
                action.Init(agent, memory);
            }
        }
        
        public List<IAIAction<T, R>> GetActions()
        {
            return actions;
        }

        public void DisableActions()
        {
            throw new System.NotImplementedException();
        }

        public void EnableActions()
        {
            throw new System.NotImplementedException();
        }

        public void AddAction<TAction>() where TAction : IAIAction<T, R>, new()
        {
            if (actions.Any(s => s is T)) return;
            
            var action = new TAction();
            action.Init(agent, memory);
            actions.Add(action);
        }

        public void RemoveAction<TAction>() where TAction : IAIAction<T, R>
        {
            actions.RemoveAll(s => s is TAction);
        }
    }
}