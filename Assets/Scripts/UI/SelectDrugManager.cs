using Drugs;
using Events.ApplyDrug;
using Events.Signal;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class SelectDrugManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider intensitySlider;

        [SerializeField] private Text intensityValueText;
        [SerializeField] private Button clearAllButton;

        [Inject]
        private ApplyDrugEventChannel _applyDrugEventChannel;

        [Inject(Id = GameSignalId.ClearDrugs)]
        private SignalEventChannel _clearDrugsEventChannel;

        private float _currentIntensity = 1.0f;

        private void Start()
        {
            UpdateIntensityDisplay(_currentIntensity);
        }

        private void OnEnable()
        {
            if (intensitySlider != null)
                intensitySlider.onValueChanged.AddListener(OnIntensityChanged);

            if (clearAllButton != null)
                clearAllButton.onClick.AddListener(OnClearAllButtonClicked);
        }

        private void OnDisable()
        {
            if (intensitySlider != null)
                intensitySlider.onValueChanged.RemoveListener(OnIntensityChanged);

            if (clearAllButton != null)
                clearAllButton.onClick.RemoveListener(OnClearAllButtonClicked);
        }

        public void HandleDrugButtonCLicked(string drugName)
        {
            switch (drugName)
            {
                case "cocaine":
                    OnDrugSelected(DrugType.Cocaine);
                    break;
                case "ethanol":
                    OnDrugSelected(DrugType.Ethanol);
                    break;
                case "tetrodotoxin":
                    OnDrugSelected(DrugType.Tetrodotoxin);
                    break;
                case "morphine":
                    OnDrugSelected(DrugType.Morphine);
                    break;
                case "ketamine":
                    OnDrugSelected(DrugType.Ketamine);
                    break;
                default:
                    Debug.LogWarning($"Unknown drug name: {drugName}");
                    break;
            }
        }

        private void OnDrugSelected(DrugType drugType)
        {
            _applyDrugEventChannel.Raise(new ApplyDrugData
            {
                drugType = drugType,
                intensity = _currentIntensity
            });
        }

        private void OnIntensityChanged(float value)
        {
            _currentIntensity = value;
            UpdateIntensityDisplay(value);
        }

        private void UpdateIntensityDisplay(float value)
        {
            if (intensityValueText != null) intensityValueText.text = $"Intensity: {value:F2}";
        }

        private void OnClearAllButtonClicked()
        {
            _clearDrugsEventChannel.Raise();
        }
    }
}