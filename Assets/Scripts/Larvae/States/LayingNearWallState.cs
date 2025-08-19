using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.States
{
    [Serializable]
    public class LayingNearWallState : BaseLarvaState
    {
        [SerializeField] private float layingTime = 5f;
        [SerializeField] private float noWallFoundGiveUpTime = 10f;

        [SerializeField] private float wallDetectionDistance = 2f;

        [SerializeField] private float wallFollowingSpeed = 0.5f;
        private bool _foundWall;
        public override string StateName => "LayingNearWall";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _foundWall = false;
            stateMachine.LarvaController.SoftChangeMovementMultiplier(1f).Forget();

            stateMachine.LarvaController.OnSegmentCollision += (_, _, _) =>
            {
                stateMachine.ForceTransitionToState("LayingDown");
            };
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState > noWallFoundGiveUpTime)
            {
                stateMachine.TransitionToDefaultState();
                return;
            }

            if (_foundWall) return;

            var larva = stateMachine.LarvaController;
            larva.StartMoving(larva.targetDirection * wallFollowingSpeed);

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
                Debug.DrawRay(position, direction * wallDetectionDistance, Color.red, 1f);
                var hit = Physics2D.Raycast(position, direction, wallDetectionDistance, LayerMask.GetMask("Default"));
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

        public override bool CanTransitionTo(string stateName)
        {
            return stateName == "Spastic" || stateName == "KHole" || TimeInState >= layingTime;
        }
    }
}