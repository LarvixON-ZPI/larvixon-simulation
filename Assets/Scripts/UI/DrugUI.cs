using System.Collections.Generic;
using System.Linq;
using Drugs;
using Larvae;
using Larvae.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DrugUI : MonoBehaviour
    {
        [Header("Drug Configuration")]
        [SerializeField] private List<DrugEffect> availableDrugs = new();

        [SerializeField] private LarvaSimulation larvaSimulation;

        [Header("UI References")]
        [SerializeField] private Text currentStateText;

        [SerializeField] private Slider dosageSlider;
        [SerializeField] private Text dosageText;
        [SerializeField] private GameObject drugControlPanel;
        [SerializeField] private Text larvaNameText;
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private Text movementModifierText;
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private Text statusText;
        [SerializeField] private Text activeDrugsCountText;
        [SerializeField] private Image canChangeDirectionIndicator;
        [SerializeField] private Image canMoveIndicator;
        [SerializeField] private Text activeEffectsSummaryText;
        [SerializeField] private GameObject drugUIPanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openButton;

        [Header("Movement Stat Sliders")]
        [SerializeField] private Slider speedSlider;

        [SerializeField] private Text speedLabel;
        [SerializeField] private Slider coordinationSlider;
        [SerializeField] private Text coordinationLabel;
        [SerializeField] private Slider randomnessSlider;
        [SerializeField] private Text randomnessLabel;
        [SerializeField] private Slider directionStabilitySlider;
        [SerializeField] private Text directionStabilityLabel;
        [SerializeField] private Slider segmentSyncSlider;
        [SerializeField] private Text segmentSyncLabel;
        [SerializeField] private Slider headForceSlider;
        [SerializeField] private Text headForceLabel;
        [SerializeField] private Slider restoreForceSlider;
        [SerializeField] private Text restoreForceLabel;

        [Header("Colors")]
        [SerializeField] private Color normalTextColor = Color.white;

        [SerializeField] private Color warningTextColor = Color.yellow;
        [SerializeField] private Color dangerTextColor = Color.red;

        [Header("Runtime")]
        [SerializeField] private float currentDosage = 1f;

        private readonly Dictionary<string, (Slider slider, Text label)> _movementStats = new();
        private readonly List<Larva> _targetLarvae = new();
        private DrugSystem _drugSystem;
        private Larva _targetLarva;

        private void Awake()
        {
            MapMovementStats();
        }

        private void Start()
        {
            InitializeLarvae();
            SetupEventHandlers();
            UpdateDosageDisplay();
        }

        private void Update()
        {
            UpdateAllStats();
        }

        public void ToggleDrugUI(bool show)
        {
            drugUIPanel.SetActive(show);

            closeButton.gameObject.SetActive(show);
            openButton.gameObject.SetActive(!show);
        }

        private void InitializeLarvae()
        {
            if (larvaSimulation == null) return;

            var larvae = FindObjectsByType<Larva>(FindObjectsSortMode.None).ToList();
            _targetLarvae.AddRange(larvae);

            if (_targetLarvae.Count <= 0) return;

            _targetLarva = _targetLarvae[0];
            _drugSystem = _targetLarva.GetComponent<DrugSystem>();
        }

        private void SetupEventHandlers()
        {
            if (dosageSlider == null) return;

            dosageSlider.onValueChanged.RemoveListener(OnDosageChanged);
            dosageSlider.onValueChanged.AddListener(OnDosageChanged);
            dosageSlider.value = currentDosage;
        }

        private void MapMovementStats()
        {
            _movementStats.Clear();
            TryAddStat("Speed", speedSlider, speedLabel);
            TryAddStat("Coordination", coordinationSlider, coordinationLabel);
            TryAddStat("Randomness", randomnessSlider, randomnessLabel);
            TryAddStat("Direction Stability", directionStabilitySlider, directionStabilityLabel);
            TryAddStat("Segment Sync", segmentSyncSlider, segmentSyncLabel);
            TryAddStat("Head Force", headForceSlider, headForceLabel);
            TryAddStat("Restore Force", restoreForceSlider, restoreForceLabel);
        }

        private void TryAddStat(string key, Slider slider, Text label)
        {
            if (slider != null && label != null)
                _movementStats[key] = (slider, label);
        }

        private void OnDosageChanged(float value)
        {
            currentDosage = value;
            UpdateDosageDisplay();
        }

        private void UpdateDosageDisplay()
        {
            if (dosageText == null) return;
            dosageText.text = $"Dosage: {currentDosage:P0}";
            dosageText.color = currentDosage switch
            {
                >= 1f => dangerTextColor,
                >= 0.7f => warningTextColor,
                _ => normalTextColor
            };
        }

        public void ApplyDrug(int drugIndex)
        {
            if (drugIndex < 0 || drugIndex >= availableDrugs.Count) return;
            var effect = availableDrugs[drugIndex];
            ApplyDrugToAllLarvae(effect);
        }

        public void ClearAllDrugs()
        {
            ClearAllDrugsFromLarvae();
        }

        private void ApplyDrugToAllLarvae(DrugEffect drugEffect)
        {
            var appliedCount = 0;
            foreach (var larva in _targetLarvae)
            {
                if (larva == null) continue;
                larva.AddDrugEffect(drugEffect, currentDosage);
                appliedCount++;
            }

            if (statusText != null)
                statusText.text = $"Applied {drugEffect.drugName} to {appliedCount} larvae";
        }

        private void ClearAllDrugsFromLarvae()
        {
            var cleared = 0;
            foreach (var larva in _targetLarvae)
            {
                if (larva == null) continue;
                larva.ClearAllDrugEffects();
                cleared++;
            }

            if (statusText != null)
                statusText.text = $"Cleared drugs from {cleared} larvae";
        }

        private void UpdateAllStats()
        {
            if (!_targetLarva || !_drugSystem) return;

            UpdateBasicStats();
            UpdateMovementStats();
            UpdateStatusIndicators();
            UpdateActiveEffectsSummary();
        }

        private void UpdateBasicStats()
        {
            if (larvaNameText)
                larvaNameText.text = $"Larva: {_targetLarva.name}";

            var activeDrugsCount = _drugSystem.ActiveDrugs?.Count ?? 0;
            if (activeDrugsCountText)
                activeDrugsCountText.text = $"Active Drugs: {activeDrugsCount}";

            if (movementModifierText)
            {
                var influenced = activeDrugsCount > 0;
                movementModifierText.text = influenced ? "Status: Under Drug Influence" : "Status: Normal";
                movementModifierText.color = influenced ? dangerTextColor : normalTextColor;
            }

            if (!currentStateText) return;

            var stateMachine = _targetLarva.GetComponent<LarvaStateMachine>();
            if (stateMachine?.CurrentState != null)
                currentStateText.text = $"State: {stateMachine.CurrentState.StateName}";
        }

        private void UpdateMovementStats()
        {
            var modifier = _drugSystem.CurrentModifier;
            SetMovementStat("Speed", modifier.speedMultiplier);
            SetMovementStat("Coordination", modifier.coordinationMultiplier);
            SetMovementStat("Randomness", modifier.randomnessMultiplier);
            SetMovementStat("Direction Stability", modifier.directionStability);
            SetMovementStat("Segment Sync", modifier.segmentSyncMultiplier);
            SetMovementStat("Head Force", modifier.headForceMultiplier);
            SetMovementStat("Restore Force", modifier.restoreForceMultiplier);
        }

        private void SetMovementStat(string movementStatName, float value)
        {
            if (!_movementStats.TryGetValue(movementStatName, out var stat)) return;
            stat.slider.value = value;
            stat.label.text = $"{movementStatName}: {value:F2}";
            var deviation = Mathf.Abs(value - 1f);
            stat.label.color = deviation switch
            {
                > 1f => dangerTextColor,
                > 0.3f => warningTextColor,
                _ => normalTextColor
            };
        }

        private void UpdateStatusIndicators()
        {
            var modifier = _drugSystem.CurrentModifier;
            if (canMoveIndicator)
                canMoveIndicator.color = modifier.canMove ? Color.green : dangerTextColor;
            if (canChangeDirectionIndicator)
                canChangeDirectionIndicator.color = modifier.canChangeDirection ? Color.green : dangerTextColor;
        }

        private void UpdateActiveEffectsSummary()
        {
            if (!activeEffectsSummaryText) return;
            var list = _drugSystem.ActiveDrugs;
            if (list == null || list.Count == 0)
            {
                activeEffectsSummaryText.text = "No Active Effects";
                activeEffectsSummaryText.color = normalTextColor;
                return;
            }

            activeEffectsSummaryText.text = string.Join(", ",
                list.Select(d => $"{d.Effect.drugName}({d.CurrentIntensity:F2})"));

            var maxIntensity = list.Max(d => d.CurrentIntensity);
            activeEffectsSummaryText.color = maxIntensity switch
            {
                > 0.8f => dangerTextColor,
                > 0.4f => warningTextColor,
                _ => normalTextColor
            };
        }
    }
}