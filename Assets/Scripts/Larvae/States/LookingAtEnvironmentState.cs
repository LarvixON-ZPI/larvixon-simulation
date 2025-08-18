using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    [Serializable]
    public class LookingAtEnvironmentState : BaseLarvaState
    {
        [SerializeField] private float headTurningSpeed = 90f;
        [SerializeField] private float lookingDuration = 2f;
        [SerializeField] private float lookingRange = 120f;

        private float _currentLookAngle;
        private bool _isLookingLeft;
        private float _targetLookAngle;
        public override string StateName => "LookingAtEnvironment";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            stateMachine.LarvaController.StopMoving();

            _isLookingLeft = Random.value > 0.5f;
            _targetLookAngle = _isLookingLeft ? -lookingRange : lookingRange;
            _currentLookAngle = 0f;
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            var larva = stateMachine.LarvaController;

            _currentLookAngle = Mathf.MoveTowards(_currentLookAngle, _targetLookAngle, headTurningSpeed * deltaTime);

            if (Mathf.Approximately(_currentLookAngle, _targetLookAngle))
            {
                _isLookingLeft = !_isLookingLeft;
                _targetLookAngle = _isLookingLeft ? -lookingRange : lookingRange;
            }

            var baseDirection = larva.targetDirection;
            var lookDirection = Quaternion.Euler(0, 0, _currentLookAngle) * baseDirection;
            larva.SetMovementDirection(lookDirection);

            if (TimeInState >= lookingDuration) stateMachine.TransitionToState("Moving");
        }

        public override bool CanTransitionTo(string stateName)
        {
            return stateName == "Spastic" || stateName == "KHole" ||
                   TimeInState >= lookingDuration * 0.3f;
        }
    }
}