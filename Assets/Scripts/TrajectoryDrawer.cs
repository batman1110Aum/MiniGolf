using UnityEngine;

namespace MiniGolf
{
    /// <summary>
    /// Predicts and draws an approximate trajectory using a LineRenderer.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryDrawer : MonoBehaviour
    {
        [SerializeField] private int _sampleCount = 30;
        [SerializeField] private float _timeStep = 0.05f;

        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            if (_lr == null) _lr = gameObject.AddComponent<LineRenderer>();
            _lr.positionCount = 0;
            _lr.enabled = false;
        }

        public void Show()
        {
            if (_lr) _lr.enabled = true;
        }

        public void Hide()
        {
            if (_lr)
            {
                _lr.enabled = false;
                _lr.positionCount = 0;
            }
        }

        /// <summary>
        /// Draw predicted arc points.
        /// supply initialVelocity in world-space (units/sec).
        /// </summary>
        public void Draw(Vector3 startPos, Vector3 initialVelocity)
        {
            if (_lr == null) return;

            _lr.positionCount = _sampleCount;
            for (int i = 0; i < _sampleCount; i++)
            {
                float t = i * _timeStep;
                Vector3 point = startPos + initialVelocity * t + 0.5f * Physics.gravity * (t * t);
                _lr.SetPosition(i, point);
            }
        }
    }
}
