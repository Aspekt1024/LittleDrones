using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI.Core
{
    // Memory holds everything the agent currently knows about the world and itself.
    // It is typically modified by a sensor, performing an action, or being acted upon.
    // Memory is the storage of state and does not perform any actions or observations.
    public class Memory<T, R> : IMemory<T, R>
    {
        private IAIAgent<T, R> agent;
        
        private readonly Dictionary<object, object> memory = new Dictionary<object, object>();
        
        public void Init(IAIAgent<T, R> agent)
        {
            this.agent = agent;
        }

        public void Reset()
        {
            memory.Clear();
        }

        public object Get(object key)
        {
            if (memory.ContainsKey(key))
            {
                return memory[key];
            }
            return null;
        }

        public V Get<K, V>(K key)
        {
            if (memory.ContainsKey(key))
            {
                return (V)memory[key];
            }
            return default;
        }

        /// <summary>
        /// Retrieves the memory value and returns true if the memory key exists.
        /// If the memory key doesn't exist, this returns false and the default value is given.
        /// </summary>
        /// <param name="key">The memory key</param>
        /// <param name="value">The memory value</param>
        /// <returns>success</returns>
        public bool TryGet(T key, out R value)
        {
            if (memory.ContainsKey(key))
            {
                value = (R)memory[key];
                return true;
            }

            value = default;
            return false;
        }

        public void Set(object key, object value)
        {
            if (memory.ContainsKey(key))
            {
                memory[key] = value;
            }
            else
            {
                memory.Add(key, value);
            }
        }

        public void Remove(object key)
        {
            memory.Remove(key);
        }
    }
}