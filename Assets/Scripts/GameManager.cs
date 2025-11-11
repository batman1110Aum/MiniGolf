using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.XInput;

namespace MiniGolf
{
    /// <summary>
    /// Coordinates gameplay events: wiring input -> ball -> timer -> holes.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private BallController _ball;
        [SerializeField] private InputController _input;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private TimerController _timer;
        [SerializeField] private Transform _ballSpawnPoint;
        [SerializeField] private HoleController[] _holes;
        [SerializeField] private float _repositionRadius = 3f;

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnAimStart += HandleAimStart;
                _input.OnAimUpdate += HandleAimUpdate;
                _input.OnAimEnd += HandleAimEnd;
            }

            if (_ball != null)
            {
                _ball.OnBallLaunched += HandleBallLaunched;
                _ball.OnBallStopped += HandleBallStopped;
            }
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnAimStart -= HandleAimStart;
                _input.OnAimUpdate -= HandleAimUpdate;
                _input.OnAimEnd -= HandleAimEnd;
            }

            if (_ball != null)
            {
                _ball.OnBallLaunched -= HandleBallLaunched;
                _ball.OnBallStopped -= HandleBallStopped;
            }
        }

        private void HandleAimStart()
        {
            _ball?.StartAiming();
        }

        private void HandleAimUpdate(Vector2 dragScreen)
        {
            _ball?.UpdateAiming(dragScreen, _mainCamera);
        }

        private void HandleAimEnd(Vector2 dragScreen)
        {
            // Perform the shot. Holes should not reposition until after the shot resolves.
            _ball?.Shoot(dragScreen, _mainCamera);
        }

        private void HandleBallLaunched()
        {
            // Optionally disable input while ball is moving; leaving active so player can prepare next shot visually.
        }

        private void HandleBallStopped()
        {
            // no-op here; ball stays until enters hole or user shoots again
        }

        /// <summary>
        /// Called by HoleTrigger when ball enters a hole.
        /// </summary>
        public void OnBallEnteredHole(HoleController hole)
        {
            if (hole == null) return;

            if (_timer != null)
            {
                if (hole.CurrentType == HoleType.Good) _timer.AddTime(5f);
                else _timer.RemoveTime(10f);
            }

            StartCoroutine(PostHoleSequence());
        }

        private IEnumerator PostHoleSequence()
        {
            // small delay for VFX/sound
            yield return new WaitForSeconds(0.2f);

            if (_ball != null && _ballSpawnPoint != null)
            {
                _ball.ResetBall(_ballSpawnPoint.position);
            }

            RepositionHoles();
        }

        private void RepositionHoles()
        {
            if (_holes == null) return;

            foreach (var hole in _holes)
            {
                if (hole == null) continue;

                Vector3 newPos = new Vector3(
                    Random.Range(-_repositionRadius, _repositionRadius),
                    hole.transform.position.y,
                    Random.Range(-_repositionRadius, _repositionRadius)
                );

                hole.transform.position = newPos;
            }
        }

        public void OnTimerEnded()
        {
            // stop gameplay; show game over UI, etc.
            if (_timer != null) _timer.StopTimer();
            Debug.Log("Game Over - Timer ended");
            // Optionally: disable input (if _input exists, you can disable the component)
            if (_input != null) _input.enabled = false;
        }
    }
}
