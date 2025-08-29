using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    [Serializable]
    public class LayingDownState : BaseLarvaState
    {
        [SerializeField] private float maxLayingTime = 60f;

        [SerializeField] private float minLayingTime = 15f;

        [SerializeField] private float slowDownTime = 5f;

        private float _layingDuration;

        public override string StateName => "LayingDown";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            stateMachine.LarvaController.SoftChangeMovementMultiplier(slowDownTime, 0f).Forget();

            _layingDuration = Random.Range(minLayingTime, maxLayingTime);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState >= _layingDuration) stateMachine.TransitionToDefaultState();
        }

        public override bool CanTransitionTo(string stateName)
        {
            return TimeInState >= minLayingTime * 0.5f && stateName == "Moving";
        }
    }
}