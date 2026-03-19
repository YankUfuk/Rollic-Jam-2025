using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor.SceneManagement;

public class ContainerWinManager : MonoBehaviour
{
    [SerializeField] private List<WinStage> stages = new List<WinStage>();
    [SerializeField] private int currentStageIndex = 0;

    public event Action OnWin;
    public event Action<int> OnStageChanged; // send new stage index

    private void OnEnable()
    {
        // Subscribe to container changes
        foreach (var stage in stages)
        {
            foreach (var c in stage.containers)
            {
                if (c != null)
                    c.OnFillStateChanged += HandleContainerChanged;
            }
        }
    }

    private void OnDisable()
    {
        // Unsubscribe
        foreach (var stage in stages)
        {
            foreach (var c in stage.containers)
            {
                if (c != null)
                    c.OnFillStateChanged -= HandleContainerChanged;
            }
        }
    }

    private void HandleContainerChanged(IFillableContainer container)
    {
        EvaluateCurrentStage();
    }

    private void EvaluateCurrentStage()
    {
        if (currentStageIndex < 0 || currentStageIndex >= stages.Count)
            return;

        var stage = stages[currentStageIndex];

        bool stageComplete = stage.mode == WinStageMode.AllAtOnce
            ? AreAllFilledNow(stage)
            : AreAllFilledRegardlessOfTiming(stage);

        if (!stageComplete)
            return;

        // Move to next stage
        currentStageIndex++;
        OnStageChanged?.Invoke(currentStageIndex);

        if (currentStageIndex >= stages.Count)
        {
            // All stages complete → Level win
            Debug.Log("Win condition met!");
            OnWin?.Invoke();
            EventManager.Instance?.TriggerEvent(new WinEvent(stage.id, currentStageIndex - 1));
        }
        else
        {
            // Optional:
            // - Reset containers in next stage
            // - Trigger FX, UI, etc.
        }
    }

    private bool AreAllFilledRegardlessOfTiming(WinStage stage)
    {
        foreach (var c in stage.containers)
        {
            if (c == null) continue;
            if (!c.IsFilled) return false;
        }
        return true;
    }

    private bool AreAllFilledNow(WinStage stage)
    {
        // For now "all at once" = at this evaluation moment, all are filled.
        // If you need stricter timing windows, you can extend this.
        return AreAllFilledRegardlessOfTiming(stage);
    }
}

public enum WinStageMode
{
    AllAtOnce,  // all containers in this stage must be filled at the same time
    AnyOrder    // all containers must be filled at some point; order doesn’t matter
}

[Serializable]
public class WinStage
{
    public string id;
    public WinStageMode mode = WinStageMode.AnyOrder;
    public List<Container> containers = new List<Container>();
}

public struct WinEvent
{
    public string stageId;
    public int stageIndex;
    public WinEvent(string id, int index)
    {
        stageId = id;
        stageIndex = index;
    }
}