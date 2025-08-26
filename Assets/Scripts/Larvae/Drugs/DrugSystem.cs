using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Larvae.Drugs
{
    public class DrugSystem : MonoBehaviour
    {
        private readonly List<ActiveDrugEffect> _activeDrugs = new();
        private Larva _larva;

        public MovementModifier CurrentModifier { get; private set; } = MovementModifier.Normal;
        public bool HasActiveDrugs => _activeDrugs.Count > 0;

        private void Awake()
        {
            _larva = GetComponent<Larva>();
        }

        private void Update()
        {
            UpdateActiveDrugs();
            UpdateMovementModifier();
        }

        public void AddDrug(DrugEffect drugEffect, float dosage = 1f)
        {
            var activeDrug = new ActiveDrugEffect(drugEffect, dosage);
            _activeDrugs.Add(activeDrug);

            ProgressDrugPhase(activeDrug).Forget();
        }

        private async UniTaskVoid ProgressDrugPhase(ActiveDrugEffect activeDrug)
        {
            var totalDuration = activeDrug.Effect.onsetTime + activeDrug.Effect.duration +
                                activeDrug.Effect.comedownTime;
            var elapsed = 0f;

            while (elapsed < totalDuration && _activeDrugs.Contains(activeDrug))
            {
                elapsed += Time.deltaTime;

                float currentIntensity;
                if (elapsed < activeDrug.Effect.onsetTime)
                {
                    var onsetProgress = elapsed / activeDrug.Effect.onsetTime;
                    currentIntensity = Mathf.Lerp(0f, activeDrug.Effect.maxIntensity, onsetProgress);
                }
                else if (elapsed < activeDrug.Effect.onsetTime + activeDrug.Effect.duration)
                {
                    var peakProgress = (elapsed - activeDrug.Effect.onsetTime) / activeDrug.Effect.duration;
                    currentIntensity = activeDrug.Effect.maxIntensity *
                                       activeDrug.Effect.intensityCurve.Evaluate(peakProgress);
                }
                else
                {
                    var comedownProgress = (elapsed - activeDrug.Effect.onsetTime - activeDrug.Effect.duration) /
                                           activeDrug.Effect.comedownTime;
                    currentIntensity = Mathf.Lerp(activeDrug.Effect.maxIntensity, 0f, comedownProgress);
                }

                activeDrug.CurrentIntensity = currentIntensity * activeDrug.Dosage;

                await UniTask.Yield();
            }

            _activeDrugs.Remove(activeDrug);
        }

        private void UpdateActiveDrugs()
        {
            foreach (var activeDrug in _activeDrugs)
                activeDrug.Effect.ApplyCustomEffects(_larva, activeDrug.CurrentIntensity);
        }

        private void UpdateMovementModifier()
        {
            if (_activeDrugs.Count == 0)
            {
                CurrentModifier = MovementModifier.Normal;
                return;
            }

            var modifier = MovementModifier.Normal;

            foreach (var activeDrug in _activeDrugs)
            {
                var drugModifier = activeDrug.Effect.GetMovementModifier(activeDrug.CurrentIntensity);
                modifier = CombineModifiers(modifier, drugModifier, activeDrug.CurrentIntensity);

                //var preferredState = activeDrug.Effect.GetPreferredState(activeDrug.CurrentIntensity);
                //if (_stateMachine.CurrentState?.StateName != preferredState)
                //    _stateMachine.TransitionToState(preferredState);
            }

            CurrentModifier = modifier;
        }

        private MovementModifier CombineModifiers(MovementModifier baseModifier, MovementModifier drugModifier,
            float intensity)
        {
            return new MovementModifier
            {
                speedMultiplier = Mathf.Lerp(baseModifier.speedMultiplier, drugModifier.speedMultiplier, intensity),
                coordinationMultiplier = Mathf.Lerp(baseModifier.coordinationMultiplier,
                    drugModifier.coordinationMultiplier, intensity),
                randomnessMultiplier = Mathf.Max(baseModifier.randomnessMultiplier,
                    drugModifier.randomnessMultiplier * intensity),
                directionStability = Mathf.Lerp(baseModifier.directionStability, drugModifier.directionStability,
                    intensity),
                segmentSyncMultiplier = Mathf.Lerp(baseModifier.segmentSyncMultiplier,
                    drugModifier.segmentSyncMultiplier, intensity),
                headForceMultiplier = Mathf.Lerp(baseModifier.headForceMultiplier, drugModifier.headForceMultiplier,
                    intensity),
                restoreForceMultiplier = Mathf.Lerp(baseModifier.restoreForceMultiplier,
                    drugModifier.restoreForceMultiplier, intensity),
                canMove = baseModifier.canMove && drugModifier.canMove,
                canChangeDirection = baseModifier.canChangeDirection && drugModifier.canChangeDirection
            };
        }

        public void ClearAllDrugs()
        {
            _activeDrugs.Clear();
        }
    }
}