namespace Larvae.States
{
    public interface ILarvaState
    {
        string StateName { get; }
        void Enter(LarvaStateMachine stateMachine);
        void Update(LarvaStateMachine stateMachine, float deltaTime);
        void Exit(LarvaStateMachine stateMachine);
        bool CanTransitionTo(string stateName);
    }
}