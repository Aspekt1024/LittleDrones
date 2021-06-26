using System.Collections.Generic;

namespace Aspekt.AI.Planning
{
    public class AStar<L, V> : IAStar<L, V>
    {
        private readonly List<AINode<L, V>> nullNodes = new List<AINode<L, V>>();
        private readonly List<AINode<L, V>> openNodes = new List<AINode<L, V>>();
        private readonly List<AINode<L, V>> closedNodes = new List<AINode<L, V>>();

        private AINode<L, V> currentNode;

        public bool FindActionPlan(IAIAgent<L, V> agent, IAIGoal<L, V> goal)
        {
            InitialiseNodeLists(agent, goal);

            while (!currentNode.ConditionsMet())
            {
                FindNeighbouringNodes();

                if (openNodes.Count == 0) return false;

                currentNode = FindCheapestNode();
                closedNodes.Add(currentNode);
                openNodes.Remove(currentNode);
            }

            return true;
        }

        public Queue<IAIAction<L, V>> GetActionPlan()
        {
            var queue = new Queue<IAIAction<L, V>>();
            while (currentNode.GetAction() != null)
            {
                queue.Enqueue(currentNode.GetAction());
                currentNode = currentNode.GetParent();
            }

            var actionPlan = new Queue<IAIAction<L, V>>();
            while (queue.Count > 0)
            {
                actionPlan.Enqueue(queue.Dequeue());
            }

            return actionPlan;
        }
        
        private void InitialiseNodeLists(IAIAgent<L, V> agent, IAIGoal<L, V> goal)
        {
            nullNodes.Clear();
            openNodes.Clear();
            closedNodes.Clear();

            currentNode = new AINode<L, V>(agent, goal);
            closedNodes.Add(currentNode);

            foreach (var action in agent.Actions.GetActions())
            {
                nullNodes.Add(new AINode<L, V>(agent, goal, action));
            }
        }
        
        private void FindNeighbouringNodes()
        {
            for (int i = nullNodes.Count - 1; i >= 0; i--)
            {
                if (!nullNodes[i].GetAction().CheckComponents()) continue;
                if (!AchievesPrecondition(nullNodes[i])) continue;
                
                nullNodes[i].Update(currentNode);
                openNodes.Add(nullNodes[i]);
                nullNodes.Remove(nullNodes[i]);
            }
        }

        private bool AchievesPrecondition(AINode<L, V> node)
        {
            foreach (var effect in node.GetAction().GetEffects())
            {
                var preconditions = currentNode.GetState().GetPreconditions();
                if (preconditions.ContainsKey(effect.Key) && preconditions[effect.Key].Equals(effect.Value))
                {
                    return true;
                }
            }
            return false;
        }

        private AINode<L, V> FindCheapestNode()
        {
            var cheapestNode = openNodes[0];
            for (int i = 1; i < openNodes.Count; i++)
            {
                if (openNodes[i].GetFCost() < cheapestNode.GetFCost())
                {
                    cheapestNode = openNodes[i];
                }
            }
            return cheapestNode;
        }
    }
}