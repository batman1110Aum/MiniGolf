using UnityEngine;
using UnityEngine.UI;

namespace MiniGolf
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private Text _timerText;

        public void SetTimerText(string text)
        {
            if (_timerText != null) _timerText.text = text;
        }
    }
}
