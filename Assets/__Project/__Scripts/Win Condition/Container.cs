using System;
using UnityEngine;
using MoreMountains.Feedbacks;   // Make sure MMFeedbacks / Feel is imported

public class Container : MonoBehaviour, IFillableContainer
{
    [Header("Fill Settings")]
    [SerializeField] private float requiredFill = 1f;   // how much is needed (can be >1)
    [SerializeField] private float currentFill = 0f;

    [Header("Liquid Volume")]
    [SerializeField] private LiquidVolumeFX.LiquidVolume lv;
    [SerializeField] private float liquidLerpSpeed = 2f;   // how fast visual fill catches up

    [Header("Feedbacks (MMFeel)")]
    [Tooltip("Played on each successful AddProgress while not yet full. Use for subtle scale/glow.")]
    [SerializeField] private MMF_Player onFillTickFeedback;

    [Tooltip("Played once when the container becomes completely full. Use for big feedback.")]
    [SerializeField] private MMF_Player onFilledFeedback;

    public event Action<IFillableContainer> OnFillStateChanged;

    private float _visualLevel;      // what we actually feed into LiquidVolume
    private bool _wasFilled = false; // track first time we reach full

    public float Progress01 => Mathf.Clamp01(requiredFill <= 0 ? 1f : currentFill / requiredFill);
    public bool IsFilled => Progress01 >= 1f;

    // ----------------- Lifecycle -----------------

    private void Awake()
    {
        if (lv == null)
            lv = GetComponent<LiquidVolumeFX.LiquidVolume>();

        // Initialize visual level
        _visualLevel = lv != null ? lv.level : Progress01;
    }

    private void Update()
    {
        // Smoothly lerp visual fill to logical progress
        if (lv == null)
            return;

        float target = Progress01;

        if (!Mathf.Approximately(_visualLevel, target))
        {
            _visualLevel = Mathf.MoveTowards(
                _visualLevel,
                target,
                liquidLerpSpeed * Time.deltaTime
            );

            lv.level = _visualLevel;
        }
    }

    // ----------------- Logic API -----------------

    public void AddProgress(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
            return;

        bool wasFilledBefore = IsFilled;
        float oldProgress = Progress01;

        // Update logical fill
        currentFill = Mathf.Clamp(currentFill + amount, 0f, requiredFill);
        float newProgress = Progress01;

        if (!Mathf.Approximately(oldProgress, newProgress))
        {
            OnFillStateChanged?.Invoke(this);
        }

        // While filling but not full yet → subtle positive feedback
        if (!IsFilled && newProgress > oldProgress)
        {
            // e.g. small scale punch + glow while filling
            if (onFillTickFeedback != null)
            {
                onFillTickFeedback.PlayFeedbacks(transform.position);
            }
        }

        // First time reaching full → big positive feedback
        if (!wasFilledBefore && IsFilled)
        {
            _wasFilled = true;

            if (onFilledFeedback != null)
            {
                onFilledFeedback.PlayFeedbacks(transform.position);
            }
        }
    }

    public void ResetFill()
    {
        currentFill = 0f;
        _visualLevel = 0f;
        _wasFilled = false;

        if (lv != null)
        {
            lv.level = _visualLevel;
        }

        OnFillStateChanged?.Invoke(this);
    }
}
