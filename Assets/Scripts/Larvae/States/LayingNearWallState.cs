using System;
using UnityEngine;

namespace Larvae.States
{
    [Serializable]
    public class LayingNearWallState : BaseLarvaState
    {
        [SerializeField] private float layingTime = 5f;
        [SerializeField] private float noWallFoundGiveUpTime = 2f;

        [SerializeField] private float wallDetectionDistance = 2f;

        [SerializeField] private float wallFollowingSpeed = 0.5f;
        private bool _foundWall;
        public override string StateName => "LayingNearWall";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);
            _foundWall = false;

            FindNearestWall(stateMachine);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (!_foundWall)
            {
                var larva = stateMachine.LarvaController;
                larva.StartMoving(larva.targetDirection * wallFollowingSpeed);

                if (TimeInState > noWallFoundGiveUpTime) stateMachine.TransitionToState("Moving");
            }
            else
            {
                stateMachine.LarvaController.StopMoving();

                if (TimeInState >= layingTime) stateMachine.TransitionToState("Moving");
            }
        }

        private void FindNearestWall(LarvaStateMachine stateMachine)
        {
            var larva = stateMachine.LarvaController;
            var position = larva.transform.position;

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

            var wallDirection = (closestHit.Value - (Vector2)position).normalized;
            larva.SetMovementDirection(wallDirection);
            _foundWall = true;
        }

        public override bool CanTransitionTo(string stateName)
        {
            return stateName == "Spastic" || stateName == "KHole" || TimeInState >= layingTime;
        }
    }
}