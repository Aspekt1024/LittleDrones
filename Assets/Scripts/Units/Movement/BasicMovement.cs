using Aspekt.AI;
using UnityEngine;

namespace Aspekt.Drones
{
    public class BasicMovement : IMovement
    {
        private const float SqrProximityThreshold = 4f * 4f;    // Slow down at this distance
        private const float SqrTargetReachedThreshold = 0.7f;      // Stop at this distance
        private const float MaxSpeed = 3f;
        private const float MinSpeed = 1f;

        private readonly Rigidbody body;
        
        private Transform targetTf;
        private Vector3 targetPos;
        private bool finalPoint;
        private bool targetReached;

        private enum States
        {
            None, Moving, Stopping
        }
        private States state = States.None;

        public BasicMovement(Rigidbody body)
        {
            this.body = body;
        }
        
        public void MoveTo(Transform target, bool isFinalPoint)
        {
            finalPoint = isFinalPoint;
            targetTf = target;
            state = States.Moving;
        }

        public void MoveTo(Vector3 position, bool isFinalPoint)
        {
            finalPoint = isFinalPoint;
            targetTf = null;
            targetPos = position;
            state = States.Moving;
        }

        public void Tick(float deltaTime)
        {
            if (state != States.Moving) return;

            if (targetTf != null)
            {
                targetPos = targetTf.position;
            }

            Vector3 distVector = targetPos - body.position;
            distVector.y = 0f; // force to horizontal plane
            float sqrDist = Vector3.SqrMagnitude(distVector);

            float speed = MaxSpeed;
            if (sqrDist < SqrTargetReachedThreshold)
            {
                targetReached = true;
                Stop();
            }
            else if (sqrDist < SqrProximityThreshold)
            {
                float distRatio = (sqrDist - SqrTargetReachedThreshold) / (SqrProximityThreshold - SqrTargetReachedThreshold);
                speed = Mathf.Lerp(MinSpeed, MaxSpeed, distRatio);
            }
            body.velocity = distVector.normalized * speed;
        }

        public void Run()
        {
            state = States.Moving;
        }

        public void Stop(bool immediate = false)
        {
            if (immediate)
            {
                body.velocity = Vector3.zero;
                return;
            }
            
            // TODO slow down over time
            state = States.Stopping;
            body.velocity = Vector3.zero;
            state = States.None;
        }

        public bool TargetReached() => targetReached;
    }
}