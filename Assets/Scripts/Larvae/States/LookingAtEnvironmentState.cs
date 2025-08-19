using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.States
{
    [Serializable]
    public class LookingAtEnvironmentState : BaseLarvaState
    {
        [SerializeField] private float gazeHoldTime = 2f;
        [SerializeField] private float totalStateTime = 10f;
        [SerializeField] private float minAngleBetweenLooks = 60f;
        [SerializeField] private float headNudgeDuration = 0.3f;
        [SerializeField] private float headNudgeMovementMultiplier = 0.2f;
        private Vector2 _currentLookDirection;

        private float _nextGazeChange;
        private CancellationTokenSource _nudgeCts;

        public override string StateName => "LookingAtEnvironment";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            stateMachine.LarvaController.StopMoving();
            stateMachine.LarvaController.SoftChangeMovementMultiplier(0.15f, 0f).Forget();

            _currentLookDirection = stateMachine.LarvaController.targetDirection.sqrMagnitude > 0.001f
                ? stateMachine.LarvaController.targetDirection.normalized
                : Vector2.right;

            stateMachine.LarvaController.SetMovementDirection(_currentLookDirection);
            _nextGazeChange = gazeHoldTime;

            NudgeHead(stateMachine.LarvaController, _currentLookDirection);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState >= totalStateTime)
            {
                stateMachine.TransitionToDefaultState();
                return;
            }

            if (TimeInState < _nextGazeChange) return;

            var larva = stateMachine.LarvaController;
            var newDir = PickDifferentDirection(larva, _currentLookDirection, minAngleBetweenLooks);
            _currentLookDirection = newDir;
            larva.SetMovementDirection(newDir);

            _nextGazeChange = TimeInState + gazeHoldTime;

            NudgeHead(larva, newDir);
        }

        public override void Exit(LarvaStateMachine stateMachine)
        {
            base.Exit(stateMachine);

            _nudgeCts?.Cancel();
            _nudgeCts?.Dispose();
            _nudgeCts = null;
        }

        public override bool CanTransitionTo(string stateName)
        {
            return stateName is "Spastic" or "KHole" || TimeInState >= totalStateTime;
        }

        private static Vector2 PickDifferentDirection(Larva larva, Vector2 current, float minAngleDeg)
        {
            for (var i = 0; i < 10; i++)
            {
                var candidate = larva.GetDesiredDirection();
                if (Vector2.Angle(current, candidate) >= minAngleDeg)
                    return candidate;
            }

            return -current.normalized;
        }

        private void NudgeHead(Larva larva, Vector2 dir)
        {
            _nudgeCts?.Cancel();
            _nudgeCts?.Dispose();
            _nudgeCts = new CancellationTokenSource();
            var token = _nudgeCts.Token;

            NudgeAsync().Forget();
            return;

            async UniTaskVoid NudgeAsync()
            {
                try
                {
                    larva.StartMoving(dir);
                    larva.SoftChangeMovementMultiplier(0.05f, headNudgeMovementMultiplier).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(headNudgeDuration), cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                }
                finally
                {
                    larva.StopMoving();

                    larva.SoftChangeMovementMultiplier(0.1f, 0f).Forget();
                }
            }
        }
    }
}