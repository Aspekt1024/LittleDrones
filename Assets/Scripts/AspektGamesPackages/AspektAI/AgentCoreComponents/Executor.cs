using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    public class Executor<L, V> : IExecutor<L, V>
    {
        private IAIAgent<L, V> agent;
        private Queue<IAIAction<L, V>> actionPlan;
        private IAIAction<L, V> currentAction;
        private IAIGoal<L, V> currentGoal;

        private readonly IStateMachine<L, V> stateMachine;
        
        public event Action OnPlanFinished = delegate { };

        private enum States
        {
            None, Running, Paused, Stopped,
        }

        private States state = States.None;
        
        public Executor(IAIAgent<L, V> agent)
        {
            this.agent = agent;
            stateMachine = new StateMachine<L, V>(agent);
        }

        public void Tick(float deltaTime)
        {
            if (currentAction == null) return;

            if (state == States.Running)
            {
                currentAction.Tick(deltaTime);
                stateMachine.Tick(deltaTime);
            }
        }

        public void ExecutePlan(Queue<IAIAction<L, V>> newActionPlan, IAIGoal<L, V> goal)
        {
            agent.LogInfo(this, "Executing plan...");
            
            if (state == States.Paused || state == States.Running)
            {
                Stop();
            }

            actionPlan = newActionPlan;
            currentGoal = goal;

            goal.SetupGoal();
            
            state = States.Running;
            BeginNextAction();
        }
        
        public void Stop()
        {
            if (currentAction != null)
            {
                stateMachine.Stop();
            }
            state = States.Stopped;
        }

        public void Pause()
        {
            if (state == States.Running)
            {
                state = States.Paused;
            }
        }

        public void Resume()
        {
            if (state == States.Paused)
            {
                state = States.Running;
            }
        }

        private void BeginNextAction()
        {
            agent.LogTrace(this, "starting next action");
            if (actionPlan.Count == 0)
            {
                currentAction = null;
                state = States.Stopped;
                OnPlanFinished?.Invoke();
                agent.LogInfo(this, "no more actions. Plan has finished");
                return;
            }
            
            currentAction = actionPlan.Dequeue();
            agent.LogInfo(this, $"action starting: {currentAction}");
            var success = currentAction.Enter(stateMachine, OnActionSuccess, OnActionFailure);
            if (!success)
            {
                OnActionFailure();
            }
        }

        private void OnActionSuccess()
        {
            agent.LogInfo(this, $"action succeeded: {currentAction}");
            foreach (var effect in currentAction.GetEffects())
            {
                agent.Memory.Set(effect.Key, effect.Value);
            }

            bool goalAchieved = true;
            foreach (var condition in currentGoal.GetConditions())
            {
                if (agent.Memory.IsMatch(condition.Key, condition.Value)) continue;
                
                goalAchieved = false;
                break;
            }

            if (goalAchieved)
            {
                agent.LogKeyInfo(this, $"{currentGoal} achieved");
                Stop();
                OnPlanFinished?.Invoke();
            }
            else
            {
                BeginNextAction();
            }
        }

        private void OnActionFailure()
        {
            agent.LogInfo(this, $"action failed: {currentAction}");
            Stop();
            OnPlanFinished?.Invoke();
        }
    }
}