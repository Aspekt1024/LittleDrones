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

        protected override void OnRun()
        {
            // Apply sensor and ability effects before planning, because turning off the agent clears memory
            foreach (var sensor in Sensors.GetSensors())
            {
                var effects = sensor.Effects;
                foreach (var effect in effects)
                {
                    Memory.Set(effect, true);
                }
            }
            
            // TODO set these through abilities
            Memory.Set(AIAttributes.CanMove, true); // TODO create movement ability
            Memory.Set(AIAttributes.CanPickupItems, true); // TODO create grabber
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