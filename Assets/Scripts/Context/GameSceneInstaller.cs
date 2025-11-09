using Events.ApplyDrug;
using Events.MoveLarvaToPoint;
using Events.Signal;
using UnityEngine;
using Zenject;

namespace Context
{
    public class GameSceneInstaller : MonoInstaller
    {
        [System.Serializable]
        public struct SignalBinding
        {
            public GameSignalId id;
            public SignalEventChannel channel;
        }

        [Header("Event Channels")] [SerializeField]
        public SignalBinding[] signalBindings;
        public ApplyDrugEventChannel applyDrugEventChannel;
        public SetDestinationForLarva setDestinationForLarva;

        public override void InstallBindings()
        {
            foreach (var binding in signalBindings)
            {
                Container.BindInstance(binding.channel).WithId(binding.id);
            }

            Container.BindInstance(setDestinationForLarva);
            Container.BindInstance(applyDrugEventChannel);
        }
    }
}