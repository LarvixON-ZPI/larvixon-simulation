using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.States
{
    public class LayingNearWallState : BaseLarvaState
    {
        private const float LayingTime = 5f;
        private const float NoWallFoundGiveUpTime = 10f;
        private const float WallDetectionDistance = 2f;
        private const float WallFollowingSpeed = 0.5f;

        private Action<int, float, Vector2> _collisionHandler;
        private bool _foundWall;
        public override string StateName => "LayingNearWall";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _foundWall = false;
            stateMachine.LarvaController.SoftChangeMovementMultiplier(1f).Forget();

            _collisionHandler = (_, _, _) => stateMachine.ForceTransitionToState("LayingDown");
            stateMachine.LarvaController.OnSegmentCollision += _collisionHandler;
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState > NoWallFoundGiveUpTime)
            {
                stateMachine.TransitionToDefaultState();
                return;
            }

            if (_foundWall) return;

            var larva = stateMachine.LarvaController;
            larva.StartMoving(larva.targetDirection * WallFollowingSpeed);

            CheckIfNearWall(stateMachine);
        }

        private void CheckIfNearWall(LarvaStateMachine stateMachine)
        {
            var larva = stateMachine.LarvaController;
            var position = larva.GetHeadPosition();

            var directions = new[]
            {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                Vector2.up + Vector2.left, Vector2.up + Vector2.right,
                Vector2.down + Vector2.left, Vector2.down + Vector2.right
            };

            var closestDistance = float.MaxValue;
            Vector2? closestHit = null;
            foreach (var direction in directions)
            {
                Debug.DrawRay(position, direction * WallDetectionDistance, Color.red, 1f);
                var hit = Physics2D.Raycast(position, direction, WallDetectionDistance, LayerMask.GetMask("Default"));
                if (!hit.collider) continue;

                if (!(hit.distance < closestDistance)) continue;

                closestDistance = hit.distance;
                closestHit = hit.point;
            }

            if (!closestHit.HasValue)
            {
                _foundWall = false;
                return;
            }

            var wallDirection = (closestHit.Value - position).normalized;
            larva.SetMovementDirection(wallDirection);
            larva.SoftChangeMovementMultiplier(.3f, .1f).Forget();
            _foundWall = true;
        }

        public override void Exit(LarvaStateMachine stateMachine)
        {
            base.Exit(stateMachine);

            if (_collisionHandler != null)
                stateMachine.LarvaController.OnSegmentCollision -= _collisionHandler;
        }

        public override bool CanTransitionTo(string stateName)
        {
            return TimeInState >= LayingTime;
        }
    }
}