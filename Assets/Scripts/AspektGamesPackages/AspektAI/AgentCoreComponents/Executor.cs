using System;
using System.Collections.Generic;

namespace Aspekt.AI.Internal
{
    public class Executor<L, V> : IExecutor<L, V>
    {
        private readonly IAIAgent<L, V> agent;
        
        private Queue<IAIAction<L, V>> actionPlan;
        private IAIGoal<L, V> currentGoal;
        private IAIAction<L, V> _currentAction;

        private IAIAction<L, V> CurrentAction
        {
            get => _currentAction;
            set
            {
                if (_currentAction != null)
                {
                    _currentAction.OnActionFailed -= OnActionFailure;
                    _currentAction.OnActionSucceeded -= OnActionSuccess;
                }

                _currentAction = value;
            }
        }

        private readonly IStateMachine<L, V> stateMachine;
        
        public event Action OnActionPlanComplete = delegate { };

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
            if (CurrentAction == null) return;

            if (state == States.Running)
            {
                CurrentAction.Tick(deltaTime);
                stateMachine.Tick(deltaTime);
            }
        }

        public void ExecutePlan(Queue<IAIAction<L, V>> newActionPlan, IAIGoal<L, V> goal)
        {
            agent.LogTrace(this, "plan executing started");
            
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
            if (CurrentAction != null)
            {
                stateMachine.Stop();
                CurrentAction = null;
            }

            if (currentGoal != null)
            {
                currentGoal.ResetGoal();
                currentGoal = null;
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

        public string GetStatus()
        {
            if (state == States.None)
            {
                return "Inactive";
            }
            var status = state.ToString();
            if (currentGoal == null) return status;
            
            status += "\n\n<b>Goal:</b> " + currentGoal;
            if (actionPlan == null || CurrentAction == null) return status;
            
            status += "\n\n<b>Action Plan:</b>\n" + _currentAction;
            foreach (var a in actionPlan)
            {
                status += "\n" + a;
            }
            
            return status;
        }

        private void BeginNextAction()
        {
            agent.LogTrace(this, "starting next action");
            stateMachine.Stop();
            
            if (actionPlan.Count == 0)
            {
                CurrentAction = null;
                state = States.Stopped;
                OnActionPlanComplete?.Invoke();
                agent.LogInfo(this, "no more actions. Plan has finished");
                return;
            }
            
            CurrentAction = actionPlan.Dequeue();
            agent.LogTrace(this, $"action starting: {CurrentAction}");
            var success = CurrentAction.Enter(stateMachine);
            if (success)
            {
                CurrentAction.OnActionFailed += OnActionFailure;
                CurrentAction.OnActionSucceeded += OnActionSuccess;
                stateMachine.Start();
            }
            else
            {
                OnActionFailure();
            }
        }

        private void OnActionSuccess()
        {
            agent.LogTrace(this, $"action succeeded: {CurrentAction}");
            foreach (var effect in CurrentAction.GetEffects())
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
                OnActionPlanComplete?.Invoke();
            }
            else
            {
                BeginNextAction();
            }
        }

        private void OnActionFailure()
        {
            agent.LogInfo(this, $"action failed: {CurrentAction}");
            Stop();
            OnActionPlanComplete?.Invoke();
        }
    }
}