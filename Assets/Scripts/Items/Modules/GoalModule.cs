using Aspekt.AI;
using Aspekt.Items;

namespace Aspekt.Drones
{   
    public abstract class GoalModule : InventoryItem, IDroneModule
    {
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
            goal = CreateGoal();
        }

        protected abstract AIGoal<AIAttributes, object> CreateGoal();
    }
}