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
        private readonly IAstarAI ai;
        
        private Transform targetTf;
        private Vector3 targetPos;

        private enum States
        {
            None, AwaitingPath, Moving, Stopping
        }
        private States state = States.None;

        public bool Enabled { get; set; } = true;

        public GroundMovement(Rigidbody body, IAstarAI ai)
        {
            this.body = body;
            this.ai = ai;

            ai.maxSpeed = MaxSpeed;
        }
        
        public void MoveTo(Transform target)
        {
            if (state == States.Moving && target == targetTf) return;
            
            targetTf = target;
            targetPos = targetTf.position;
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
            
            if (state != States.Moving || targetTf == null) return;

            if (Vector3.SqrMagnitude(targetTf.position - targetPos) > SqrTargetRecalculationDistance)
            {
                CalculatePath();
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
    }
}