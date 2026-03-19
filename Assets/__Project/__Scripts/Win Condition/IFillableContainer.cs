using UnityEngine;
using System;

public interface IFillableContainer
{
    void AddProgress(float amount);   // progressive filling
    float Progress01 { get; }         // 0–1 normalized
    bool IsFilled { get; }

    event Action<IFillableContainer> OnFillStateChanged;
}
