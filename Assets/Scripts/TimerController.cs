using UnityEngine;
using UnityEngine.UI;

namespace MiniGolf
{
    /// <summary>
    /// Simple countdown timer. Uses UnityEngine.UI.Text for display.
    /// </summary>
    public class TimerController : MonoBehaviour
    {
        [SerializeField] private float _timeRemaining = 30f;
        [SerializeField] private Text _timerText;
        [SerializeField] private GameManager _gameManager;

        private bool _isRunning = true;

        private void Update()
        {
            if (!_isRunning) return;

            _timeRemaining -= Time.deltaTime;
            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                _isRunning = false;
                if (_gameManager != null) _gameManager.OnTimerEnded();
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_timerText != null)
            {
                int secs = Mathf.CeilToInt(_timeRemaining);
                _timerText.text = secs.ToString();
            }
        }

        public void AddTime(float seconds)
        {
            _timeRemaining += seconds;
        }

        public void RemoveTime(float seconds)
        {
            _timeRemaining -= seconds;
            if (_timeRemaining < 0f) _timeRemaining = 0f;
        }

        public float TimeRemaining => _timeRemaining;
        public bool IsRunning => _isRunning;

        public void StopTimer() => _isRunning = false;
        public void StartTimer() => _isRunning = true;
    }
}
