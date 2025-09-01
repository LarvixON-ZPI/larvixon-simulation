using Larvae;
using UnityEngine;

namespace Drugs
{
    public abstract class DrugEffect : ScriptableObject
    {
        [Header("Drug Properties")] public string drugName;

        [SerializeField] [TextArea(3, 6)] private string description;

        [Header("Effect Parameters")] [Range(0f, 1f)]
        public float maxIntensity = 1f;

        [Range(0.1f, 1800f)] public float duration = 1500f;

        [Range(0.1f, 1800f)] public float onsetTime = 300f;

        [Range(0.1f, 1800f)] public float comedownTime = 300f;

        [Header("Movement Modifiers")] [Range(0f, 2f)]
        public float maxSpeedMultiplier = 3f;

        [Range(0f, 5f)] public float maxRandomnessMultiplier = 1f;

        [Range(0f, 10f)] public float maxHeadForceMultiplier = 1f;

        [Range(0f, 5f)] public float maxRestoreForceMultiplier = 1f;

        [Range(0f, 1f)] public float minCoordinationMultiplier = 1f;

        [Range(0f, 1f)] public float minDirectionStability = 1f;

        [Range(0f, 1f)] public float minSegmentSyncMultiplier = 1f;

        // Defines lethal effect timing: negative values indicate no lethal effect, 
        // positive values represent the proportion of drug duration after which death occurs
        public AnimationCurve lethalTimeRange = AnimationCurve.EaseInOut(0, -1, 1, 0.5f);
        [SerializeField] private float maxSafeDose = 0.5f;

        public AnimationCurve onsetIntensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public float TotalDurationTime => onsetTime + duration + comedownTime;

        // not counting comedownTime
        public float MainDurationTime => onsetTime + duration;

        public float DeathProbability
        {
            get
            {
                const int steps = 1000;
                var positiveCount = 0;

                for (var i = 0; i <= steps; i++)
                {
                    var t = i / (float)steps;
                    if (lethalTimeRange.Evaluate(t) >= 0f)
                        positiveCount++;
                }

                return positiveCount / (steps + 1f);
            }
        }

        public bool IsSafeDose(float dose)
        {
            return dose <= maxSafeDose;
        }

        public virtual string GetPreferredState(float intensity)
        {
            return "Moving";
        }

        public virtual void ApplyCustomEffects(Larva larva, float intensity)
        {
        }

        public virtual MovementModifier GetMovementModifier(float intensity)
        {
            var speedMultiplier = Mathf.Lerp(1f, maxSpeedMultiplier, intensity);
            var coordinationMultiplier = Mathf.Lerp(1f, minCoordinationMultiplier, intensity);
            var randomnessMultiplier = Mathf.Lerp(1f, maxRandomnessMultiplier, intensity);
            var directionStability = Mathf.Lerp(1f, minDirectionStability, intensity);
            var segmentSyncMultiplier = Mathf.Lerp(1f, minSegmentSyncMultiplier, intensity);
            var headForceMultiplier = Mathf.Lerp(1f, maxHeadForceMultiplier, intensity);
            var restoreForceMultiplier = Mathf.Lerp(1f, maxRestoreForceMultiplier, intensity);

            return new MovementModifier
            {
                SpeedMultiplier = speedMultiplier,
                SegmentCoordinationMultiplier = coordinationMultiplier,
                RandomnessMultiplier = randomnessMultiplier,
                DirectionStability = directionStability,
                SegmentSyncMultiplier = segmentSyncMultiplier,
                HeadForceMultiplier = headForceMultiplier,
                RestoreForceMultiplier = restoreForceMultiplier,
                CanMove = true,
                CanChangeDirection = true
            };
        }
    }
}