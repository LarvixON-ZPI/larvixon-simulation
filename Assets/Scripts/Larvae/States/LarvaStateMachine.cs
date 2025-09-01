using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Larvae.States
{
    public class LarvaStateMachine : MonoBehaviour
    {
        private readonly Dictionary<string, ILarvaState> _states = new();

        public Dictionary<string, float> WeightedStates = new()
        {
            { "LayingDown", 0.5f },
            { "LookingAtEnvironment", 0.3f },
            { "LayingNearWall", 0.2f },
            { "Moving", 1f }
        };

        public ILarvaState CurrentState { get; private set; }

        public Larva LarvaController { get; private set; }

        private void Awake()
        {
            LarvaController = GetComponent<Larva>();
        }

        private void Update()
        {
            CurrentState?.Update(this, Time.deltaTime);
        }

        public void RegisterState(ILarvaState state)
        {
            _states.TryAdd(state.StateName, state);
        }

        public void TransitionToState(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var newState))
            {
                Debug.LogWarning($"State {stateName} not found in state machine");
                return;
            }

            if (CurrentState != null && !CurrentState.CanTransitionTo(stateName))
            {
                Debug.LogWarning($"Cannot transition from {CurrentState.StateName} to {stateName}");
                return;
            }

            CurrentState?.Exit(this);
            CurrentState = newState;
            CurrentState.Enter(this);
        }

        public void ForceTransitionToState(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var newState))
            {
                Debug.LogWarning($"State {stateName} not found in state machine");
                return;
            }

            if (!CurrentState.OverridableByForce) return;

            CurrentState?.Exit(this);
            CurrentState = newState;
            CurrentState.Enter(this);
        }

        public void StartStateMachine([CanBeNull] string initialStateName = null)
        {
            initialStateName ??= GetDefaultState();

            TransitionToState(initialStateName);
        }

        public void TransitionToDefaultState()
        {
            TransitionToState(GetDefaultState());
        }

        public void TransitionToNextState([CanBeNull] string skippedState = null)
        {
            var nextState = CalculateNextState(skippedState);
            TransitionToState(nextState);
        }

        private float GetTotalStateWeight([CanBeNull] string skippedState = null)
        {
            return skippedState == null
                ? WeightedStates.Values.Sum()
                : WeightedStates.Where(kv => kv.Key != skippedState).Sum(kv => kv.Value);
        }

        private string CalculateNextState([CanBeNull] string skippedState = null)
        {
            var val = Random.value * GetTotalStateWeight(skippedState);

            var possibleStates = skippedState == null
                ? WeightedStates
                : WeightedStates.Where(kv => kv.Key != skippedState).ToDictionary(kv => kv.Key, kv => kv.Value);

            var cumulative = 0f;
            foreach (var stateWeight in possibleStates)
            {
                cumulative += stateWeight.Value;

                if (val <= cumulative) return stateWeight.Key;
            }

            return WeightedStates.Keys.Last();
        }

        private static string GetDefaultState()
        {
            return "Moving";
        }

        public T GetState<T>(string stateName) where T : class, ILarvaState
        {
            return _states.TryGetValue(stateName, out var state) ? state as T : null;
        }
    }
}