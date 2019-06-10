using System;
using Aspekt.AI;

namespace Aspekt.Drones
{
    public class DroneAIAgent : AIAgent<AIAttributes, object>
    {
        private enum Views
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
        
        protected override void OnActionPlanComplete()
        {
            base.OnActionPlanComplete();
            LogInfo(this, "action plan complete - finding next goal");
            QueueGoalCalculation();
        }
    }
}