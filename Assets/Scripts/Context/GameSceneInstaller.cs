using Events.Signal;
using UnityEngine;
using Zenject;

namespace Context
{
    public class GameSceneInstaller : MonoInstaller
    {
        [System.Serializable]
        private struct SignalBinding
        {
            public GameSignalId id;
            public SignalEventChannel channel;
        }

        [Header("Event Channels")] [SerializeField]
        private SignalBinding[] signalBindings;

        public override void InstallBindings()
        {
            foreach (var binding in signalBindings)
            {
                Container.BindInstance(binding.channel)
                    .WithId(binding.id)
                    .AsCached();
            }
        }
    }
}