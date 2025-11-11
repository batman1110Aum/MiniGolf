using UnityEngine;
using System;

namespace MiniGolf
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        public event Action OnBallStopped;
        public event Action OnBallLaunched;

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private TrajectoryDrawer _trajectory;
        [SerializeField] private float _powerMultiplier = 0.0025f; // tweak for feel
        [SerializeField] private float _stopSpeedThreshold = 0.05f;

        private bool _isLaunched;

        private void Reset()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!_isLaunched) return;

            if (_rb.linearVelocity.magnitude <= _stopSpeedThreshold && _rb.IsSleeping())
            {
                _isLaunched = false;
                OnBallStopped?.Invoke();
            }
        }

        /// <summary>
        /// Call when player begins aiming.
        /// </summary>
        public void StartAiming()
        {
            _trajectory?.Show();
        }

        /// <summary>
        /// Update the visual trajectory while the player drags.
        /// dragScreenVec is a screen-space vector from touch start to current position.
        /// </summary>
        public void UpdateAiming(Vector2 dragScreenVec, Camera cam)
        {
            if (_trajectory == null) return;

            // Convert screen drag to a world horizontal direction.
            // Use simple mapping: screen x -> world x, screen y -> world z (inverted).
            Vector3 dir = new Vector3(-dragScreenVec.x, 0f, -dragScreenVec.y);
            Vector3 initialVel = dir * _powerMultiplier * 1000f; // scale for drawing
            _trajectory.Draw(transform.position, initialVel);
        }

        /// <summary>
        /// Shoot the ball. dragScreenVec is the final drag vector (screen-space).
        /// </summary>
        public void Shoot(Vector2 dragScreenVec, Camera cam)
        {
            _trajectory?.Hide();

            // Map screen drag to world-space force; negative because drag -> pull-back input.
            Vector3 force = new Vector3(-dragScreenVec.x, 0f, -dragScreenVec.y) * _powerMultiplier;

            // Normalize/limit magnitude to avoid huge forces on large screens
            float mag = force.magnitude;
            if (mag > 0f)
            {
                float max = 1.5f;
                if (mag > max) force = force.normalized * max;
            }

            // Apply as impulse
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(force, ForceMode.Impulse);

            _isLaunched = true;
            OnBallLaunched?.Invoke();
        }

        public void ResetBall(Vector3 spawnPosition)
        {
            _trajectory?.Hide();
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            transform.position = spawnPosition;
            _isLaunched = false;
        }
    }
}
