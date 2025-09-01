using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

namespace Larvae.States
{
    public class LayingDownState : BaseLarvaState
    {
        private const float MaxLayingTime = 60f;
        private const float MinLayingTime = 15f;
        private const float SlowDownTime = 5f;

        private float _layingDuration;

        public override string StateName => "LayingDown";

        public override void Enter(LarvaStateMachine stateMachine)
        {
            base.Enter(stateMachine);

            stateMachine.LarvaController.SoftChangeMovementMultiplier(SlowDownTime, 0f).Forget();

            _layingDuration = Random.Range(MinLayingTime, MaxLayingTime);
        }

        public override void Update(LarvaStateMachine stateMachine, float deltaTime)
        {
            base.Update(stateMachine, deltaTime);

            if (TimeInState >= _layingDuration) stateMachine.TransitionToDefaultState();
        }

        public override bool CanTransitionTo(string stateName)
        {
            return TimeInState >= MinLayingTime * 0.5f && stateName == "Moving";
        }
    }
}