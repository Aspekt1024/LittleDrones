using System.Collections;
using UnityEngine;

namespace Aspekt.UI
{
    public interface IUIPanel
    {

        /// <summary>
        /// Initialises the UI Panel
        /// </summary>
        void Init();
        
        /// <summary>
        /// Opens the UI panel using its animation
        /// </summary>
        void Open();
        
        /// <summary>
        /// Closes the UI panel using its animation
        /// </summary>
        void Close();
        
        /// <summary>
        /// Same as Open() but can be used synchronously in a coroutine
        /// </summary>
        IEnumerator OpenRoutine();
        
        /// <summary>
        /// Same as Close() but can be used synchronously in a coroutine
        /// </summary>
        IEnumerator CloseRoutine();
        
        /// <summary>
        /// Opens the UI panel immediately (no animation)
        /// </summary>
        void OpenImmediate();
        
        /// <summary>
        /// Closes the UI panel immediately (no animation)
        /// </summary>
        void CloseImmediate();
    }
}