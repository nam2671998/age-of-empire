using System;
using UnityEngine;

public abstract class EventChannelSO : ScriptableObject
{
    private event Action OnEventRaised;

    public void Raise()
    {
        OnEventRaised?.Invoke();
    }

    public void Register(Action listener)
    {
        OnEventRaised += listener;
    }

    public void Unregister(Action listener)
    {
        OnEventRaised -= listener;
    }

    protected virtual void OnDisable()
    {
        OnEventRaised = null;
    }
}

