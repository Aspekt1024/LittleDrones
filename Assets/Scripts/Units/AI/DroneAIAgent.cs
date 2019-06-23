using System;
using Aspekt.AI;
using UnityEngine;

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

        private IUnit unit;
        
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

        protected override void OnInit()
        {
            unit = Owner.GetComponent<IUnit>();
            if (unit == null)
            {
                Debug.LogError($"{nameof(DroneAIAgent)} requires and owner with an {nameof(IUnit)} component");
            }
        }
    }
}