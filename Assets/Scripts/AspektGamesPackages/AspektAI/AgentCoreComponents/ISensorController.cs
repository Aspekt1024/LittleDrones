using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI.Core
{
    public interface ISensorController
    {
        void Init(IAIAgent agent, IMemory memory);
        void Tick(float deltaTime);
        
        /// <summary>
        /// Returns a copy of the list of sensors in the sensor controller
        /// </summary>
        /// <returns></returns>
        List<ISensor> GetSensors();
        
        void DisableSensors();
        
        void EnableSensors();
        
        void AddSensor<T>() where T : ISensor, new();
        
        void RemoveSensor<T>() where T : ISensor;
    }
}