using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace Aspekt.Drones
{
	/// <summary>
	/// Uses AI pathfinding to move along the ground to a target
	/// </summary>
    public class GroundMovement : IMovement
    {
        private const float SqrTargetRecalculationDistance = 0.25f;
        private const float MaxSpeed = 8f;

        private readonly Rigidbody body;
        private readonly IAstarAI ai;
        
        private Transform targetTf;
        private Vector3 targetPos;
        private float targetReachedDistance;

        private readonly List<IMovement.IObserver> observers = new List<IMovement.IObserver>();
        public void RegisterObserver(IMovement.IObserver observer) => observers.Add(observer);
        public void UnregisterObserver(IMovement.IObserver observer) => observers.Remove(observer);

        private enum States
        {
            None, AwaitingPath, Moving, TargetReached, Stopping
        }
        private States state = States.None;

        public bool Enabled { get; set; } = true;

        public GroundMovement(Rigidbody body, IAstarAI ai)
        {
            this.body = body;
            this.ai = ai;

            ai.maxSpeed = MaxSpeed;
        }
        
        public void MoveTo(Transform target, float targetReachedDistance = 3f)
        {
            if (state == States.Moving && target == targetTf) return;
            
            targetTf = target;
            targetPos = targetTf.position;
            this.targetReachedDistance = targetReachedDistance;
            
            CalculatePath();
        }

        public void MoveTo(Vector3 position)
        {
            targetTf = null;
            targetPos = position;
            targetReachedDistance = 1f;
            
            CalculatePath();
        }

        public void Tick()
        {
            if (targetTf == null) return;

            if (state == States.Moving || state == States.TargetReached)
            {
                if (HasTargetMoved())
                {
                    CalculatePath();
                }

                if (state == States.Moving && HasReachedTarget())
                {
                    state = States.TargetReached;
                    ai.isStopped = true;
                    body.velocity = Vector3.zero;
                    observers.ToList().ForEach(o => o.OnTargetReached());
                }
                
                if (state == States.TargetReached)
                {
                    LookAtTarget();
                    
                    if (!HasReachedTarget())
                    {
                        state = States.Moving;
                        ai.isStopped = false;
                        ai.destination = targetTf.position;
                    }
                }
            }
        }

        public void Run()
        {
            state = States.Moving;
            if (targetTf != null)
            {
                CalculatePath();
            }
        }

        public void Stop(bool immediate = false)
        {
            targetTf = null;
            ai.isStopped = true;
            body.velocity = Vector3.zero;
            state = States.None;
        }

        private void CalculatePath()
        {
            state = States.AwaitingPath;
            ai.destination = targetPos;
            ai.SearchPath();
            ai.isStopped = false;
            state = States.Moving;
        }

        private bool HasReachedTarget()
        {
            var pos = body.transform.position;
            pos.y = 0f;

            var dist = Vector3.Distance(pos, new Vector3(targetPos.x, 0f, targetPos.z));
            return dist < targetReachedDistance;
        }

        private bool HasTargetMoved()
        {
            return Vector3.SqrMagnitude(targetTf.position - targetPos) > SqrTargetRecalculationDistance;
        }

        private void LookAtTarget()
        {
            if (targetTf != null)
            {
                var pos = body.transform.position;
                var distVector = targetTf.position - pos;
                distVector.y = pos.y;
                
                const float rotationSpeed = 15;
                var targetRot = Quaternion.LookRotation(distVector);
                body.transform.rotation = Quaternion.Slerp(body.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
    }
}