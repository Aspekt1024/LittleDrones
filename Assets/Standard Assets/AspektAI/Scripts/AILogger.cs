using System;
using System.Collections.Generic;
using UnityEngine;

namespace Aspekt.AI
{
    public enum AILogType
    {
        /// <summary>
        /// Debug message type
        /// </summary>
        Trace,
        
        /// <summary>
        /// A minor message type
        /// </summary>
        Info,
            
        /// <summary>
        /// An informational message for a key event or behaviour
        /// </summary>
        KeyInfo,
    }
    
    /// <summary>
    /// A basic logger for standard debug messages for the AI agent
    /// </summary>
    public class AILogger
    {
        public enum LogLevels
        {
            /// <summary>
            /// No messages will be logged from the AI agent
            /// </summary>
            None,
            
            /// <summary>
            /// All messages (except trace messages) will be logged from the AI agent 
            /// </summary>
            Debug,
            
            /// <summary>
            /// Only key messages will be logged from the AI agent
            /// </summary>
            Standard,
            
            /// <summary>
            /// All messages will be logged from the AI agent
            /// </summary>
            Trace
        }
        
        public readonly LogLevels LogLevel;
        
        public interface IObserver
        {
            void OnLogMessageReceived(AILogType type, string parent, string message);
        }

        private readonly List<IObserver> observers = new List<IObserver>();
        public void RegisterObserver(IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IObserver observer) => observers.Remove(observer);

        public AILogger(LogLevels level)
        {
            LogLevel = level;
        }

        public void Log<T>(AILogType type, T parent, string message)
        {
            string prefix = typeof(T).Name;
            observers.ForEach(o => o.OnLogMessageReceived(type, prefix, message));
            
            if (LogLevel == LogLevels.None) return;

            bool shouldLog = false;
            switch (type)
            {
                case AILogType.Trace:
                    shouldLog = LogLevel == LogLevels.Trace;
                    break;
                case AILogType.Info:
                    shouldLog = LogLevel == LogLevels.Debug || LogLevel == LogLevels.Trace;
                    break;
                case AILogType.KeyInfo:
                    shouldLog = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (shouldLog)
            {
                Debug.Log($"{prefix}: {message}");
            }
        }
    }
}