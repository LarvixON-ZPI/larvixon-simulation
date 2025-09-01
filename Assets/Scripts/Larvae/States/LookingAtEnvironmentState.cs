using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.States
{
    public class LookingAtEnvironmentState : BaseLarvaState
    {
        private const float GazeHoldTime = 2f;
        private const float HeadNudgeDuration = 0.3f;
        private const float HeadNudgeMovementMultiplier = 0.2f;
        private const float MinAngleBetweenLooks = 60f;
        private const float TotalStateTime = 10f;

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
            _nextGazeChange = GazeHoldTime;

            NudgeHead(stateMachine.LarvaController, _currentLookDirection);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState >= TotalStateTime)
            {
                stateMachine.TransitionToDefaultState();
                return;
            }

            if (TimeInState < _nextGazeChange) return;

            var larva = stateMachine.LarvaController;
            var newDir = PickDifferentDirection(larva, _currentLookDirection, MinAngleBetweenLooks);
            _currentLookDirection = newDir;
            larva.SetMovementDirection(newDir);

            _nextGazeChange = TimeInState + GazeHoldTime;

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
            return TimeInState >= TotalStateTime;
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
                    larva.SoftChangeMovementMultiplier(0.05f, HeadNudgeMovementMultiplier).Forget();
                    await UniTask.Delay(TimeSpan.FromSeconds(HeadNudgeDuration), cancellationToken: token);
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