using System;
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
        
        private readonly LogLevels logLevel;

        public AILogger(LogLevels level)
        {
            logLevel = level;
        }

        public void Log<T>(AILogType type, T parent, string message)
        {
            if (logLevel == LogLevels.None) return;

            string prefix = typeof(T).Name;

            bool shouldLog = false;
            switch (type)
            {
                case AILogType.Trace:
                    shouldLog = logLevel == LogLevels.Trace;
                    break;
                case AILogType.Info:
                    shouldLog = logLevel == LogLevels.Debug || logLevel == LogLevels.Trace;
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