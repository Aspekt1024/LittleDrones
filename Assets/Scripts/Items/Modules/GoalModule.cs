using System;
using Aspekt.AI;
using Aspekt.Items;

namespace Aspekt.Drones
{   
    [Serializable]
    public abstract class GoalModule : InventoryItem, IDroneModule
    {
        public int priority;
        
        private AIGoal<AIAttributes, object> goal;
        
        public void AttachTo(DroneAIAgent agent)
        {
            agent.Goals.AddGoal(goal);
        }

        public void RemoveFrom(DroneAIAgent agent)
        {
            agent.Goals.RemoveGoal(goal);
        }

        public abstract bool IsTypeMatch(GoalModule other);
        
        private void Awake()
        {
            goal = CreateGoal(priority);
        }

        protected abstract AIGoal<AIAttributes, object> CreateGoal(int priority);
    }
}