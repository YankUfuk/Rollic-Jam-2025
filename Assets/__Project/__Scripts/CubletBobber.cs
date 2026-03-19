using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace __Scripts
{
    public class CubletBobber : MonoBehaviour
    {
        [SerializeField] private float bobDistance = 0.1f;
        [SerializeField] private float bobDuration = 0.2f;

        [SerializeField] private MMF_Player bobFeedback;
        
        private Vector3 _defaultPosition;
        private Tween _bobTween;

        private void Start()
        {
            _defaultPosition = transform.position;
            GetComponentInParent<CustomRubikCube>().OnChanged += HandleOnChanged;
        }

        private void OnDestroy()
        {
            GetComponentInParent<CustomRubikCube>().OnChanged -= HandleOnChanged;
        }

        private void HandleOnChanged()
        {
            bobFeedback?.PlayFeedbacks();

        }
    }
}