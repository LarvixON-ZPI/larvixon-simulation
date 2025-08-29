using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text simulationTimeText;
        [SerializeField] private Slider simulationSpeedSlider;

        public UnityEvent<float> onSimulationSpeedChanged = new();
        private float _simulationSpeed = 1f;

        private void Update()
        {
            simulationTimeText.SetText(
                $"{Time.timeSinceLevelLoad:F2}s, {_simulationSpeed:F2}x");
        }

        private void OnEnable()
        {
            simulationSpeedSlider.onValueChanged.AddListener(OnSimulationSpeedSliderChanged);
        }

        private void OnDisable()
        {
            simulationSpeedSlider.onValueChanged.RemoveListener(OnSimulationSpeedSliderChanged);
        }

        private void OnSimulationSpeedSliderChanged(float newValue)
        {
            _simulationSpeed = Mathf.Pow(newValue, 2);

            onSimulationSpeedChanged.Invoke(_simulationSpeed);
        }
    }
}