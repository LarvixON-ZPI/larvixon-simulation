using System;
using UnityEngine;
using UnityEngine.Events;

namespace Events.ApplyDrug
{
    [CreateAssetMenu(menuName = "Events/ApplyDrugEventChannel")]
    public class ApplyDrugEventChannel : ScriptableObject
    {
        public UnityEvent<ApplyDrugData> onEventRaisedUnityEvent;
        private event Action<ApplyDrugData> OnEventRaised;

        public void RaiseEvent(ApplyDrugData data)
        {
            OnEventRaised?.Invoke(data);
            onEventRaisedUnityEvent?.Invoke(data);
        }

        public void RegisterListener(Action<ApplyDrugData> listener)
        {
            OnEventRaised += listener;
        }

        public void UnregisterListener(Action<ApplyDrugData> listener)
        {
            OnEventRaised -= listener;
        }
    }
}

