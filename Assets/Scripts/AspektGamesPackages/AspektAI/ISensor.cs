using Aspekt.AI.Core;

namespace Aspekt.AI
{
    /// <summary>
    /// A sensor gives an agent the ability to perceive the world around it, or reflect upon itself.
    /// Sensors typically modify the agent's memory based on what they perceive.
    /// </summary>
    public interface ISensor
    {
        void Init(IAIAgent agent, IMemory memory);
        void Tick(float deltaTime);
        void Enable();
        void Disable();
        
        /// <summary>
        /// Used to clean up if the sensor affects memory
        /// </summary>
        void Remove();
    }
}