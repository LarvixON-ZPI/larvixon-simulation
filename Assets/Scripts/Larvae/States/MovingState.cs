using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    public class MovingState : BaseLarvaState
    {
        private const float DirectionChangeInterval = 3f;
        private const float DirectionChangeVariance = 2f;
        private const float StateChangeProbability = 0.1f;

        private float _nextDirectionChange;

        public override string StateName => "Moving";

        private static float CalculateHeadInfluence(float x)
        {
            var y = x < 0.3f ? x * 2 : Mathf.Sqrt(x);
            return y;
        }

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            stateMachine.LarvaController.StartMoving(stateMachine.LarvaController.targetDirection);
            stateMachine.LarvaController.SoftChangeMovementMultiplier(1f).Forget();
            _nextDirectionChange = GetNextDirectionChangeTime();
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            var larva = stateMachine.LarvaController;

            if (TimeInState < _nextDirectionChange) return;

            if (Random.value < StateChangeProbability)
            {
                stateMachine.TransitionToNextState();
                return;
            }

            var newDirection =
                GetRandomDirectionBiased(larva.targetDirection, CalculateHeadInfluence(Random.value));
            larva.SetMovementDirection(newDirection);
            _nextDirectionChange = TimeInState + GetNextDirectionChangeTime();
        }

        private float GetNextDirectionChangeTime()
        {
            return DirectionChangeInterval + Random.Range(-DirectionChangeVariance, DirectionChangeVariance);
        }

        public override bool CanTransitionTo(string _)
        {
            return true;
        }
    }
}