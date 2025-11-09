using System;
using UnityEngine;
using UnityEngine.Events;

namespace Events
{
    public abstract class EventChannelBase : ScriptableObject
    {
        public UnityEvent unityEvent;
        private event Action OnEventRaised;

        public void Raise()
        {
            OnEventRaised?.Invoke();
            unityEvent?.Invoke();
        }

        public void Register(Action listener) => OnEventRaised += listener;
        public void Unregister(Action listener) => OnEventRaised -= listener;
    }

    public abstract class EventChannelBase<T> : ScriptableObject
    {
        public UnityEvent<T> unityEvent;
        private event Action<T> OnEventRaised;

        public void Raise(T data)
        {
            OnEventRaised?.Invoke(data);
            unityEvent?.Invoke(data);
        }

        public void Register(Action<T> listener) => OnEventRaised += listener;
        public void Unregister(Action<T> listener) => OnEventRaised -= listener;
    }
}