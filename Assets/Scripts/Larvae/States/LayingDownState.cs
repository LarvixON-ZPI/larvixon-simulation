using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    [Serializable]
    public class LayingDownState : BaseLarvaState
    {
        [SerializeField] private float occasionalTwitchChance = 0.1f;
        [SerializeField] private float maxLayingTime = 60f;

        [SerializeField] private float minLayingTime = 15f;

        private float _layingDuration;

        public override string StateName => "LayingDown";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            stateMachine.LarvaController.StopMoving();
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