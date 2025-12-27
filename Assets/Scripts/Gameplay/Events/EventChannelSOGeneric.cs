using System;
using UnityEngine;

public abstract class EventChannelSO<T> : ScriptableObject
{
    private event Action<T> OnEventRaised;

    public void Raise(T payload)
    {
        OnEventRaised?.Invoke(payload);
    }

    public void Register(Action<T> listener)
    {
        OnEventRaised += listener;
    }

    public void Unregister(Action<T> listener)
    {
        OnEventRaised -= listener;
    }

    protected virtual void OnDisable()
    {
        OnEventRaised = null;
    }
}

