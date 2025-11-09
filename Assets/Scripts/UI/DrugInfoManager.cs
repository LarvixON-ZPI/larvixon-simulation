using System.Collections.Generic;
using System.Linq;
using Drugs;
using JetBrains.Annotations;
using Larvae;
using Main;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class DrugInfoManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform drugInfoContainer;

        [SerializeField] private GameObject drugInfoPrefab;
        [SerializeField] [CanBeNull] private Text noDrugsText;

        [Header("Update Settings")]
        [SerializeField] private float updateInterval = 0.1f;

        private readonly Dictionary<DrugType, Text> _activeEntries = new();

        [Inject]
        private LarvaSimulation _larvaSimulation;

        private float _timeSinceLastUpdate;
        private Larva _trackedLarva;

        private void Start()
        {
            UpdateTrackedLarva();

            noDrugsText?.gameObject.SetActive(true);
        }

        private void Update()
        {
            _timeSinceLastUpdate += Time.deltaTime;

            if (_timeSinceLastUpdate < updateInterval) return;
            _timeSinceLastUpdate = 0f;
            UpdateDrugInfo();
        }

        private void OnDestroy()
        {
            ClearAllEntries();
        }

        private void UpdateTrackedLarva()
        {
            _trackedLarva = _larvaSimulation.Larvae[0];
        }

        private void UpdateDrugInfo()
        {
            if (!_trackedLarva)
            {
                UpdateTrackedLarva();
                if (!_trackedLarva)
                    return;
            }

            var drugSystem = _trackedLarva.GetComponent<DrugSystem>();

            var activeDrugs = drugSystem.ActiveDrugs;

            if (activeDrugs.Count == 0)
            {
                ClearAllEntries();
                return;
            }

            noDrugsText?.gameObject.SetActive(false);

            var activeDrugTypes = new HashSet<DrugType>();

            foreach (var activeDrug in activeDrugs)
            {
                var drugType = activeDrug.Effect.drugType;
                activeDrugTypes.Add(drugType);

                var percentage = CalculateDrugPercentage(activeDrug);

                if (!_activeEntries.TryGetValue(drugType, out var entry))
                {
                    entry = CreateDrugInfoEntry();
                    _activeEntries[drugType] = entry;
                }

                UpdateEntry(entry, activeDrug.Effect.drugName, percentage);
            }

            var toRemove = new List<DrugType>();
            foreach (var kvp in _activeEntries.Where(kvp => !activeDrugTypes.Contains(kvp.Key)))
            {
                toRemove.Add(kvp.Key);
                Destroy(kvp.Value.gameObject);
            }

            foreach (var drugType in toRemove) _activeEntries.Remove(drugType);
        }

        private static float CalculateDrugPercentage(ActiveDrugEffect activeDrug)
        {
            return activeDrug.CurrentIntensity * 100f;
        }

        private Text CreateDrugInfoEntry()
        {
            var entryObj = Instantiate(drugInfoPrefab, drugInfoContainer);
            var text = entryObj.GetComponent<Text>();

            return text;
        }

        private static void UpdateEntry(Text text, string drugName, float percentage)
        {
            if (text) text.text = $"{drugName}: {percentage:F1}%";
        }

        private void ClearAllEntries()
        {
            foreach (var entry in _activeEntries.Values.Where(entry => entry))
                Destroy(entry.gameObject);
            _activeEntries.Clear();
            noDrugsText?.gameObject.SetActive(true);
        }
    }
}