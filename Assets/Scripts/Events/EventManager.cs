using System;
using System.Collections.Generic;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<Type, Delegate> _listeners = new Dictionary<Type, Delegate>();

    public void Subscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (_listeners.TryGetValue(eventType, out Delegate existingDelegate))
        {
            _listeners[eventType] = Delegate.Combine(existingDelegate, listener);
        }
        else
        {
            _listeners[eventType] = listener;
        }
    }

    public void Unsubscribe<T>(Action<T> listener) where T : struct
    {
        Type eventType = typeof(T);
        if (_listeners.TryGetValue(eventType, out Delegate existingDelegate))
        {
            Delegate updatedDelegate = Delegate.Remove(existingDelegate, listener);
            if (updatedDelegate != null)
            {
                _listeners[eventType] = updatedDelegate;
            }
            else
            {
                _listeners.Remove(eventType);
            }
        }
    }

    public void Publish<T>(T eventData) where T : struct
    {
        Type eventType = typeof(T);
        if (_listeners.TryGetValue(eventType, out Delegate existingDelegate))
        {
            (existingDelegate as Action<T>)?.Invoke(eventData);
        }
    }
}
