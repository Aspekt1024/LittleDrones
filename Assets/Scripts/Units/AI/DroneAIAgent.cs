using System;
using Aspekt.AI;
using UnityEditor;
using UnityEditor.VersionControl;

namespace Aspekt.Drones
{
    public class DroneAIAgent : AIAgent<AIAttributes, object>
    {
        public enum Views
        {
            Execution,
            Memory,
        }

        private Views view;
        
        public string GetStatus()
        {
            string status;
            switch (view)
            {
                case Views.Execution:
                    status = GetExecutorStatus();
                    break;
                case Views.Memory:
                    status = GetMemoryStatus();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            return status;
        }

        public void SetMemoryView() => view = Views.Memory;
        public void SetExecutionView() => view = Views.Execution;

        public void SetView(Views newView)
        {
            view = newView;
        }
        
        protected override void OnActionPlanComplete()
        {
            base.OnActionPlanComplete();
            LogInfo(this, "action plan complete - finding next goal");
            QueueGoalCalculation();
        }

        private string GetMemoryStatus()
        {
            var status = "<b>Memory:</b>";
            foreach (var state in Memory.GetState())
            {
                status += $"\n{state.Key} : {state.Value}";
            }
            return status;
        }

        private string GetExecutorStatus()
        {
            var status = "<b>Executor:</b> ";
            status += executor.GetStatus();
            return status;
        }
    }
}