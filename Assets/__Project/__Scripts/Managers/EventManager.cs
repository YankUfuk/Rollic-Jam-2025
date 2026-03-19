using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using UnityEngine;
public class EventManager : MonoBehaviour
{
    [OdinSerialize]
    private Dictionary<Type, Delegate> eventTable = new Dictionary<Type, Delegate>();

    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Register<T>(Action<T> callback)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            eventTable[typeof(T)] = (Action<T>)del + callback;
        }
        else
        {
            eventTable[typeof(T)] = callback;
        }
    }

    public void Unregister<T>(Action<T> callback)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            var currentDel = (Action<T>)del - callback;

            if (currentDel == null)
            {
                eventTable.Remove(typeof(T));
            }
            else
            {
                eventTable[typeof(T)] = currentDel;
            }
        }
    }

    public void TriggerEvent<T>(T args)
    {
        if (eventTable.TryGetValue(typeof(T), out var del))
        {
            ((Action<T>)del)?.Invoke(args);
        }
        else
        {
            print("Could not find the event");
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
