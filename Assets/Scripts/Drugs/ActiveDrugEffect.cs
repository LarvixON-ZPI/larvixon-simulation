using UnityEngine;

namespace Drugs
{
    public class ActiveDrugEffect
    {
        public ActiveDrugEffect(DrugEffect effect, float dosage)
        {
            Effect = effect;
            Dosage = Mathf.Clamp01(dosage);
            CurrentIntensity = 0f;
        }

        public DrugEffect Effect { get; }
        public float Dosage { get; }
        public float CurrentIntensity { get; set; }

        public float GetLethalTime()
        {
            return Effect.lethalTimeRange.Evaluate(Random.value) * Effect.duration;
        }

        public bool IsSafe()
        {
            return Effect.IsSafeDose(Dosage);
        }
    }
}