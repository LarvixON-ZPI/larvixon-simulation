using UnityEngine;

namespace Larvae.States
{
    public class MovingState : BaseLarvaState
    {
        private float _nextDirectionChange;

        [Header("Movement Parameters")]
        public float directionChangeInterval = 2f;

        public float directionChangeVariance = 1f;
        public override string StateName => "Moving";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            stateMachine.LarvaController.StartMoving(stateMachine.LarvaController.targetDirection);
            _nextDirectionChange = GetNextDirectionChangeTime();
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            var larva = stateMachine.LarvaController;

            if (TimeInState >= _nextDirectionChange)
            {
                var newDirection = GetRandomDirectionBiased(larva.targetDirection, 0.6f);
                larva.SetMovementDirection(newDirection);
                _nextDirectionChange = TimeInState + GetNextDirectionChangeTime();
            }
        }

        private float GetNextDirectionChangeTime()
        {
            return directionChangeInterval + Random.Range(-directionChangeVariance, directionChangeVariance);
        }

        public override bool CanTransitionTo(string stateName)
        {
            return true;
        }
    }
}