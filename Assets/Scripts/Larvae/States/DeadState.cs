using System;

namespace Larvae.States
{
    [Serializable]
    public class DeadState : BaseLarvaState
    {
        public override string StateName => "Dead";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            var larva = stateMachine.LarvaController;

            larva.StopMoving();
            larva.SetMovementMultiplier(0f);
        }

        public override bool CanTransitionTo(string stateName)
        {
            return false;
        }
    }
}