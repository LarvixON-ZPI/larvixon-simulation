using Larvae;
using UnityEngine;

namespace Drugs
{
    [CreateAssetMenu(fileName = "MorphineEffect", menuName = "Drugs/Morphine Effect")]
    public class MorphineEffect : DrugEffect
    {
        [SerializeField] private float curledLayingDownWeight = 0.7f;

        public override void OnEnter(Larva larva)
        {
            base.OnEnter(larva);

            larva.StateMachine.AddWeightedState("CurledLayingDown", curledLayingDownWeight);
        }

        public override void OnExit(Larva larva)
        {
            base.OnExit(larva);

            larva.StateMachine.RemoveWeightedState("CurledLayingDown");
        }
    }
}