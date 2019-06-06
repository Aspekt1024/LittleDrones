namespace Aspekt.AI
{
    public interface IPlanner<L, V>
    {
        /// <summary>
        /// Calculates a new goal based on the AI agent's available actions
        /// </summary>
        void CalculateNewGoal();
    }
}