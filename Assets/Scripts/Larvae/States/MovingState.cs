using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    [Serializable]
    public class MovingState : BaseLarvaState
    {
        [SerializeField] private float directionChangeInterval = 3f;
        [SerializeField] private float directionChangeVariance = 2f;
        [SerializeField] private float stateChangeProbability = 0.1f;

        private float _nextDirectionChange;
        public override string StateName => "Moving";

        private float CalculateHeadInfluence(float x)
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

            if (Random.value < stateChangeProbability)
            {
                var val = Random.value;
                var newStateName = val switch
                {
                    < 0.5f => "LayingDown",
                    < 0.8f => "LookingAtEnvironment",
                    _ => "LayingNearWall"
                };
                stateMachine.TransitionToState(newStateName);
            }

            var newDirection =
                GetRandomDirectionBiased(larva.targetDirection, CalculateHeadInfluence(Random.value));
            larva.SetMovementDirection(newDirection);
            _nextDirectionChange = TimeInState + GetNextDirectionChangeTime();
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