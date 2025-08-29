using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Larvae.States
{
    public class LarvaStateMachine : MonoBehaviour
    {
        private readonly Dictionary<string, ILarvaState> _states = new();

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