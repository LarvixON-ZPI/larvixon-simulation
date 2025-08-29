namespace Larvae.States
{
    public interface ILarvaState
    {
        string StateName { get; }
        public bool OverridableByForce { get; }
        void Enter(LarvaStateMachine stateMachine);
        void Update(LarvaStateMachine stateMachine, float deltaTime);
        void Exit(LarvaStateMachine stateMachine);
        bool CanTransitionTo(string stateName);
    }
}