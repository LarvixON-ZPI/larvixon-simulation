using UnityEngine;

namespace Larvae.Drugs
{
    public abstract class DrugEffect : ScriptableObject
    {
        [Header("Drug Properties")]
        public string drugName;

        [TextArea(3, 6)] public string description;

        [Header("Effect Parameters")]
        [Range(0f, 1f)] public float maxIntensity = 1f;

        [Range(0.1f, 1800f)] public float duration = 1500f;

        [Range(0.1f, 1800f)] public float onsetTime = 300f;

        [Range(0.1f, 1800f)] public float comedownTime = 300f;

        public AnimationCurve intensityCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

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