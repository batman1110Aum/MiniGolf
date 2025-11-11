using UnityEngine;
using System.Collections;

namespace MiniGolf
{
    public enum HoleType { Good, Bad }

    /// <summary>
    /// Controls single-hole behavior: good/bad state, random switching, flashing before change.
    /// </summary>
    public class HoleController : MonoBehaviour
    {
        [SerializeField] private HoleType _currentType = HoleType.Good;
        [SerializeField] private Renderer _renderer;
        [SerializeField] private float _minChangeTime = 3f;
        [SerializeField] private float _maxChangeTime = 8f;
        [SerializeField] private float _flashDuration = 1f;
        [SerializeField] private Color _goodColor = new Color(0f, 0.5f, 1f); // blue-ish
        [SerializeField] private Color _badColor = new Color(1f, 0f, 0f);    // red

        private float _timeToChange;
        private Coroutine _flashRoutine;

        private void Reset()
        {
            _renderer = GetComponentInChildren<Renderer>();
        }

        private void Start()
        {
            if (_renderer == null) _renderer = GetComponentInChildren<Renderer>();
            ScheduleNextChange();
            ApplyVisuals();
        }

        private void Update()
        {
            _timeToChange -= Time.deltaTime;

            if (_timeToChange <= _flashDuration && _flashRoutine == null)
            {
                _flashRoutine = StartCoroutine(FlashRoutine());
            }

            if (_timeToChange <= 0f)
            {
                ToggleType();
                ScheduleNextChange();
                if (_flashRoutine != null) { StopCoroutine(_flashRoutine); _flashRoutine = null; }
                ApplyVisuals();
            }
        }

        private void ScheduleNextChange()
        {
            _timeToChange = Random.Range(_minChangeTime, _maxChangeTime);
        }

        private IEnumerator FlashRoutine()
        {
            float elapsed = 0f;
            while (elapsed < _flashDuration)
            {
                if (_renderer != null) _renderer.enabled = !_renderer.enabled;
                yield return new WaitForSeconds(0.15f);
                elapsed += 0.15f;
            }
            if (_renderer != null) _renderer.enabled = true;
            _flashRoutine = null;
        }

        private void ToggleType()
        {
            _currentType = (_currentType == HoleType.Good) ? HoleType.Bad : HoleType.Good;
        }

        private void ApplyVisuals()
        {
            if (_renderer == null) return;
            _renderer.material.color = (_currentType == HoleType.Good) ? _goodColor : _badColor;
        }

        public HoleType CurrentType => _currentType;
    }
}
