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

        // defines probability of death - Random.value of it if 0 or higher defines time from after onset to die in proportion to duration
        // if lower than 0 for all time range - no lethal effect
        public AnimationCurve lethalTimeRange = AnimationCurve.EaseInOut(0, -1, 1, 0.5f);
        [SerializeField] private float maxSafeDose = 0.5f;

        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        public bool IsSafeDose(float dose)
        {
            return dose <= maxSafeDose;
        }

        public abstract MovementModifier GetMovementModifier(float intensity);

        public virtual string GetPreferredState(float intensity)
        {
            return "Moving";
        }

        public virtual void ApplyCustomEffects(Larva larva, float intensity)
        {
        }
    }
}