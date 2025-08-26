using UnityEngine;

namespace Larvae.Drugs
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
    }
}