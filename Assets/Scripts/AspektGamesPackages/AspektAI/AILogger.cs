using System;
using UnityEngine;

namespace Aspekt.AI
{
    public enum AILogType
    {
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
            /// All messages will be logged from the AI agent 
            /// </summary>
            Debug,
            
            /// <summary>
            /// Only key messages will be logged from the AI agent
            /// </summary>
            Standard
        }
        
        public LogLevels LogLevel;

        public AILogger(LogLevels level)
        {
            LogLevel = level;
        }

        public void Log(AILogType type, string message)
        {
            if (LogLevel == LogLevels.None) return;
            
            switch (type)
            {
                case AILogType.Info:
                    if (LogLevel == LogLevels.Debug)
                    {
                        Debug.Log(message);
                    }
                    break;
                case AILogType.KeyInfo:
                    Debug.Log(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}