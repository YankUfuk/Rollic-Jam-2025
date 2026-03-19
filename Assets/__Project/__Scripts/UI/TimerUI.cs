using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace __Scripts.UI
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private int timeLimit;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private float bobScale;
        [SerializeField] private float bobDuration;

        private int _timeLeft;
        private int _oldTimeLeft;
        private float _defaultFontSize;

        private void Awake()
        {
            _defaultFontSize = text.fontSize;
        }

        void Start()
        {
            text.text = $"{timeLimit / 60}:{(timeLimit % 60):00}";
        }

        void FixedUpdate()
        {
            _timeLeft = timeLimit - Mathf.CeilToInt(Time.time);
            if (_timeLeft < 0) return;

            text.color = _timeLeft <= 30 ? Color.red : Color.white;
            
            if (_timeLeft == _oldTimeLeft) return;
            DOVirtual.Float(_defaultFontSize * bobScale, _defaultFontSize, bobDuration, v => { text.fontSize = v; });
            _oldTimeLeft = _timeLeft;
            text.text = $"{_timeLeft / 60}:{(_timeLeft % 60):00}";

            if (_timeLeft == 0)
            {
                // Trigger time's up event
                EventManager.Instance?.TriggerEvent(new TimesUpEvent());
            }
        }
    }
}

public struct TimesUpEvent
{
}
