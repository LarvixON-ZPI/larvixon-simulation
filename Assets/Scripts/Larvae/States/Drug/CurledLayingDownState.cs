using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.States.Drug
{
    public class CurledLayingDownState : BaseLarvaState
    {
        private const float MaxLayingTime = 120f;
        private const float MinLayingTime = 60f;
        private const float SlowDownTime = 15f;
        private const float TailPositionBiasDistance = 2f;

        private float _layingDuration;
        private Vector2 _tailPositionBias;

        public override string StateName => "CurledLayingDown";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            _tailPositionBias = Random.insideUnitCircle.normalized * TailPositionBiasDistance;

            stateMachine.LarvaController.SoftChangeMovementMultiplier(SlowDownTime, 0f).Forget();

            _layingDuration = Random.Range(MinLayingTime, MaxLayingTime);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            var directionToTail = stateMachine.LarvaController.GetTailPosition() -
                stateMachine.LarvaController.GetHeadPosition() + _tailPositionBias;

            stateMachine.LarvaController.SetMovementDirection(directionToTail);

            if (TimeInState >= _layingDuration) stateMachine.TransitionToDefaultState();
        }

        public override bool CanTransitionTo(string stateName)
        {
            return TimeInState >= MinLayingTime * 0.5f && stateName == "Moving";
        }
    }
}