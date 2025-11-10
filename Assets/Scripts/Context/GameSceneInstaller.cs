using Events.ApplyDrug;
using Events.MoveLarvaToPoint;
using Events.UICloseOpenAction;
using Main;
using UnityEngine;
using Zenject;

namespace Context
{
    public class GameSceneInstaller : MonoInstaller
    {
        [Header("Simulation References")] [SerializeField]
        public LarvaSimulation larvaSimulation;

        [Header("Event Channels")] [SerializeField]
        public SignalBinding[] signalBindings;

        public ApplyDrugEventChannel applyDrugEventChannel;
        public SetDestinationForLarva setDestinationForLarva;
        public UICloseOpenActionChannel uiCloseOpenActionChannel;

        public override void InstallBindings()
        {
            Container.BindInstance(larvaSimulation);

            foreach (var binding in signalBindings) Container.BindInstance(binding.channel).WithId(binding.id);

            Container.BindInstance(setDestinationForLarva);
            Container.BindInstance(applyDrugEventChannel);
            Container.BindInstance(uiCloseOpenActionChannel);
        }
    }
}