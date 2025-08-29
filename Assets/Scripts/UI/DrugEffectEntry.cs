using TMPro;
using UnityEngine;

namespace UI
{
    public class DrugEffectEntry : MonoBehaviour
    {
        [Header("Text References")]
        public TMP_Text drugNameText;

        public TMP_Text dosageText;
        public TMP_Text intensityText;

        public void UpdateEntry(string drugName, float dosage, float intensity)
        {
            drugNameText.text = drugName;

            dosageText.text = $"Dosage: {dosage:P0}";

            intensityText.text = $"Intensity: {intensity:F2}";

            intensityText.color = intensity switch
            {
                > 0.8f => Color.red,
                > 0.4f => Color.yellow,
                _ => Color.white
            };
        }
    }
}