using UnityEngine;

namespace Larvae.Drugs
{
    public abstract class DrugEffect : ScriptableObject
    {
        [Header("Drug Properties")]
        public string drugName;

        [SerializeField] [TextArea(3, 6)] private string description;

        [Header("Effect Parameters")]
        [Range(0f, 1f)] public float maxIntensity = 1f;

        [Range(0.1f, 1800f)] public float duration = 1500f;

        [Range(0.1f, 1800f)] public float onsetTime = 300f;

        [Range(0.1f, 1800f)] public float comedownTime = 300f;

        [Header("Movement Modifiers")]
        [Range(0f, 2f)] public float maxSpeedMultiplier = 3f;

        [Range(0f, 5f)] public float maxRandomnessMultiplier = 2f;

        [Range(1f, 10f)] public float maxHeadForceMultiplier = 5f;

        [Range(1f, 5f)] public float maxRestoreForceMultiplier = 2f;

        [Range(0f, 1f)] public float minCoordinationMultiplier = 0.2f;

        [Range(0f, 1f)] public float minDirectionStability = 0.1f;

        [Range(0f, 1f)] public float minSegmentSyncMultiplier = 0.3f;

        // defines probability of death - Random.value of it if 0 or higher defines time from after onset to die in proportion to duration
        // if lower than 0 for all time range - no lethal effect
        public AnimationCurve lethalTimeRange = AnimationCurve.EaseInOut(0, -1, 1, 0.5f);
        [SerializeField] private float maxSafeDose = 0.5f;

        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

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
                speedMultiplier = speedMultiplier,
                coordinationMultiplier = coordinationMultiplier,
                randomnessMultiplier = randomnessMultiplier,
                directionStability = directionStability,
                segmentSyncMultiplier = segmentSyncMultiplier,
                headForceMultiplier = headForceMultiplier,
                restoreForceMultiplier = restoreForceMultiplier,
                canMove = true,
                canChangeDirection = true
            };
        }
    }
}