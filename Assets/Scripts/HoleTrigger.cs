using UnityEngine;

namespace MiniGolf
{
    /// <summary>
    /// Add this to a hole prefab (Collider set as isTrigger). Ensures the GameManager is notified.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HoleTrigger : MonoBehaviour
    {
        [SerializeField] private HoleController _holeController;
        [SerializeField] private GameManager _gameManager;

        private void Reset()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Ball")) return;

            if (_gameManager != null && _holeController != null)
            {
                _gameManager.OnBallEnteredHole(_holeController);
            }
        }
    }
}
