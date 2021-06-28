using UnityEngine;

namespace Aspekt.Drones
{
    public interface IMovement : IAbility
    {
	    public interface IObserver
	    {
		    void OnTargetReached();
	    }

	    void RegisterObserver(IObserver observer);
	    void UnregisterObserver(IObserver observer);
	    
	    /// <summary>
	    /// Moves to a target transform
	    /// </summary>
	    void MoveTo(Transform target, float targetReachedDistance = 1f);

	    /// <summary>
	    /// Moves to a target position
	    /// </summary>
	    void MoveTo(Vector3 position);

	    /// <summary>
	    /// Called once per frame
	    /// </summary>
	    void Tick();

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