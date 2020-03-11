using UnityEngine;

namespace Aspekt.Drones
{
    public interface IMovement : IAbility
    {
	    /// <summary>
	    /// Moves to a target transform
	    /// </summary>
	    void MoveTo(Transform target);

	    /// <summary>
	    /// Moves to a target position
	    /// </summary>
	    void MoveTo(Vector3 position);

	    /// <summary>
	    /// Called once per frame
	    /// </summary>
	    /// <param name="deltaTime">The time since the last frame update</param>
	    void Tick(float deltaTime);

	    /// <summary>
	    /// Starts / Resumes movement
	    /// </summary>
	    void Run();

	    /// <summary>
	    /// Stops any movement
	    /// </summary>
	    /// <param name="immediate">Set to true if the unit should stop immediately</param>
	    void Stop(bool immediate = false);
    }
}