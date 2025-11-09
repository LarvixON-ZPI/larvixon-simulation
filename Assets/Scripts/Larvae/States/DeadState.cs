using System;
using Cysharp.Threading.Tasks;

namespace Larvae.States
{
    [Serializable]
    public class DeadState : BaseLarvaState
    {
        public override string StateName => "Dead";
        
        public DeadState()
        {
            OverridableByForce = false;
        }

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            var larva = stateMachine.LarvaController;

            larva.StopMoving();

            OnEnter(larva).Forget();
        }

        private async UniTask OnEnter(Larva larva)
        {
            await larva.SoftChangeMovementMultiplier(.1f);
            await larva.SoftChangeMovementMultiplier(20f, 0f);
        }

        public override bool CanTransitionTo(string stateName)
        {
            return false;
        }
    }
}