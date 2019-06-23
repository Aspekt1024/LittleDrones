using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Aspekt.Drones
{
	/// <summary>
	/// Uses AI pathfinding to move along the ground to a target
	/// </summary>
    public class GroundMovement : IMovement
    {
        private const float SqrTargetReachedThreshold = 0.7f;      // Stop at this distance
        private const float SqrTargetRecalculationDistance = 0.5f;
        private const float MaxSpeed = 8f;

        private readonly Rigidbody body;
        private readonly Seeker seeker;
        
        private Transform targetTf;
        private Vector3 targetPos;

        private Queue<Vector3> path;
        private Vector3 currentPoint;
        private bool isFinalPoint;
        private bool targetReached;

        private enum States
        {
            None, AwaitingPath, Moving, Stopping
        }
        private States state = States.None;

        public bool Enabled { get; set; } = true;

        public GroundMovement(Rigidbody body, Seeker seeker)
        {
            this.body = body;
            this.seeker = seeker;
        }
        
        public void MoveTo(Transform target)
        {
            if (state == States.Moving && target == targetTf) return;
            
            targetTf = target;
            targetPos = targetTf.position;
            state = States.Moving;
            CalculatePath();
        }

        public void MoveTo(Vector3 position)
        {
            targetTf = null;
            targetPos = position;
            CalculatePath();
        }

        public void Tick(float deltaTime)
        {
            if (state == States.Stopping)
            {
                body.velocity = Vector3.zero;
                state = States.None;
            }
            
            if (state != States.Moving) return;

            if (targetTf != null)
            {
                if (Vector3.SqrMagnitude(targetTf.position - targetPos) > SqrTargetRecalculationDistance)
                {
                    CalculatePath();
                    return;
                }
            }
            
            Vector3 distVector = currentPoint - body.position;
            distVector.y = 0f; // force to horizontal plane
            float sqrDist = Vector3.SqrMagnitude(distVector);

            if (sqrDist < SqrTargetReachedThreshold)
            {
                GotoNextPoint();
                return;
            }
            
            body.velocity = distVector.normalized * MaxSpeed;
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
            body.velocity = Vector3.zero;
            state = States.None;
        }

        public bool TargetReached() => targetReached;

        private void CalculatePath()
        {
            state = States.AwaitingPath;
            seeker.StartPath(body.position, targetPos, OnPathCalculated);
        }
        
        private void OnPathCalculated(Path newPath)
        {
            path = new Queue<Vector3>(newPath.vectorPath);
            targetReached = false;
            isFinalPoint = false;
            state = States.Moving;
            GotoNextPoint();
        }

        private void GotoNextPoint()
        {
            if (isFinalPoint || path == null || path.Count == 0)
            {
                targetReached = true;
                Stop();
                return;
            }
            
            currentPoint = path.Dequeue();
            if (path.Count == 0)
            {
                isFinalPoint = true;
            }
        }
    }
}