using System.Collections.Generic;

namespace Aspekt.AI.Internal
{
    public interface IMemory<L, V>
    {
        /// <summary>
        /// Initialises the Memory component of the agent
        /// </summary>
        /// <param name="agent">The agent the memory belongs to</param>
        void Init(IAIAgent<L, V> agent);
        
        /// <summary>
        /// Wipes the memory of the agent
        /// </summary>
        void Reset();

        /// <summary>
        /// Returns true if the given label matches the given value
        /// </summary>
        bool IsMatch(L label, V value);
        
        /// <summary>
        /// Returns true if the given key value pair matches the memory entry
        /// </summary>
        bool IsMatch(KeyValuePair<L, V> pair);

        /// <summary>
        /// Checks the given key is true in memory
        /// </summary>
        bool IsTrue(L label);
        
        /// <summary>
        /// Checks the given key is false in memory
        /// </summary>
        bool IsFalse(L label);

        /// <summary>
        /// Sets the memory state of the given label to the given value
        /// </summary>
        void Set(L label, V value);

        /// <summary>
        /// Gets the value of the memory state for the given label. Returns default if the label doesn't exist
        /// </summary>
        V Get(L label);

        /// <summary>
        /// Gets the value of memory state for the given label and type. Returns default if the label doesn't exist,
        /// or if the type is mismatched.
        /// </summary>
        /// <param name="label"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>(L label) where T : V;

        /// <summary>
        /// Removes the memory label from the memory state (IsMatch checks will return false until it's set again)
        /// </summary>
        void Remove(L label);

        /// <summary>
        /// Returns a reference to the memory state
        /// </summary>
        Dictionary<L, V> GetState();

        /// <summary>
        /// Returns a copy of the memory state
        /// </summary>
        Dictionary<L, V> CloneState();
    }
}