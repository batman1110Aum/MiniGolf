using UnityEngine;
using System;

namespace MiniGolf
{
    /// <summary>
    /// Handles touch (and mouse in editor) input. Raises events for aim start/update/end.
    /// </summary>
    public class InputController : MonoBehaviour
    {
        public event Action OnAimStart;
        public event Action<Vector2> OnAimUpdate; // drag vector in screen-space
        public event Action<Vector2> OnAimEnd;    // final drag vector in screen-space

        [SerializeField] private Camera _mainCamera;

        private bool _isAiming;
        private Vector2 _startPos;
        private Vector2 _lastPos;

        private void Reset()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
#if UNITY_EDITOR
            HandleMouse();
#else
            HandleTouch();
#endif
        }

        private void HandleMouse()
        {
            if (Input.GetMouseButtonDown(0))
            {
                BeginAim(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0) && _isAiming)
            {
                UpdateAim(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0) && _isAiming)
            {
                EndAim(Input.mousePosition);
            }
        }

        private void HandleTouch()
        {
            if (Input.touchCount == 0) return;

            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                BeginAim(t.position);
            }
            else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && _isAiming)
            {
                UpdateAim(t.position);
            }
            else if ((t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) && _isAiming)
            {
                EndAim(t.position);
            }
        }

        private void BeginAim(Vector2 pos)
        {
            _isAiming = true;
            _startPos = pos;
            _lastPos = pos;
            OnAimStart?.Invoke();
        }

        private void UpdateAim(Vector2 pos)
        {
            _lastPos = pos;
            Vector2 drag = pos - _startPos;
            OnAimUpdate?.Invoke(drag);
        }

        private void EndAim(Vector2 pos)
        {
            _isAiming = false;
            Vector2 drag = pos - _startPos;
            OnAimEnd?.Invoke(drag);
        }

        public bool IsAiming => _isAiming;
        public Camera MainCamera => _mainCamera;
    }
}
