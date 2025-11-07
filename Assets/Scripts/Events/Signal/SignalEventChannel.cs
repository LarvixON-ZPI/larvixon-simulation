using System;
using UnityEngine;
using UnityEngine.Events;

namespace Events.Signal
{
    [CreateAssetMenu(menuName = "Events/SignalEventChannel")]
    public class SignalEventChannel : ScriptableObject
    {
        public UnityEvent onEventRaisedUnityEvent;
        private event Action OnEventRaised;

        public void RaiseEvent()
        {
            OnEventRaised?.Invoke();
            onEventRaisedUnityEvent?.Invoke();
        }

        public void RegisterListener(Action listener)
        {
            OnEventRaised += listener;
        }

        public void UnregisterListener(Action listener)
        {
            OnEventRaised -= listener;
        }
    }
}

