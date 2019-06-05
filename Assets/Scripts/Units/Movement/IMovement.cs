using UnityEngine;

namespace Aspekt.Drones
{
    public interface IMovement
    {
	    /// <summary>
	    /// Moves to a target transform
	    /// </summary>
	    void MoveTo(Transform target, bool isFinalPoint);

	    /// <summary>
	    /// Moves to a target position
	    /// </summary>
	    void MoveTo(Vector3 position, bool isFinalPoint);

	    /// <summary>
	    /// Called once per frame
	    /// </summary>
	    /// <param name="deltaTime">The time since the last frame update</param>
	    void Tick(float deltaTime);

	    /// <summary>
	    /// Stops any movement
	    /// </summary>
	    /// <param name="immediate">Set to true if the unit should stop immediately</param>
	    void Stop(bool immediate = false);

	    
	    bool TargetReached();
    }
}