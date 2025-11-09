using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    [Serializable]
    public abstract class BaseLarvaState : ILarvaState
    {
        protected float TimeInState;
        public abstract string StateName { get; }
        public bool OverridableByForce { get; protected set; } = true;

        public virtual void Enter(LarvaStateMachine stateMachine)
        {
            TimeInState = 0f;
            Debug.Log($"Entering state: {StateName}");
        }

        public virtual void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            TimeInState += deltaTime;
        }

        public virtual void Exit(LarvaStateMachine stateMachine)
        {
            Debug.Log($"Exiting state: {StateName}");
        }

        public virtual bool CanTransitionTo(string stateName)
        {
            return true;
        }

        private static Vector2 GetRandomDirection()
        {
            return Random.insideUnitCircle.normalized;
        }

        protected static Vector2 GetRandomDirectionBiased(Vector2 currentDirection, float bias)
        {
            var random = GetRandomDirection();
            return Vector2.Lerp(random, currentDirection, bias).normalized;
        }
    }
}