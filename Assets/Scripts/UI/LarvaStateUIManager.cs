using System.Collections.Generic;
using System.Text.RegularExpressions;
using Larvae;
using Larvae.States;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LarvaStateUIManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] public Text currentStateText;

        [Header("State Management")]
        [SerializeField] private List<Larva> larvae = new();

        private void Start()
        {
            InitializeLarvae();
            UpdateCurrentStateDisplay();
        }

        private void Update()
        {
            UpdateCurrentStateDisplay();
        }

        private void InitializeLarvae()
        {
            if (larvae.Count == 0) larvae.AddRange(FindObjectsByType<Larva>(FindObjectsSortMode.None));
        }
        
        private static string FormatStateName(string stateName)
        {
            return Regex.Replace(stateName, "([a-z])([A-Z])", "$1 $2");
        }

        public void ChangeAllLarvaeToState(string stateName)
        {
            foreach (var larva in larvae)
                if (larva != null)
                {
                    var stateMachine = larva.GetComponent<LarvaStateMachine>();
                    if (stateMachine == null) continue;

                    stateMachine.ForceTransitionToState(stateName);
                    Debug.Log($"Changed larva {larva.name} to state: {stateName}");
                }
        }

        private void UpdateCurrentStateDisplay()
        {
            if (!currentStateText || larvae.Count <= 0 || !larvae[0]) return;

            var stateMachine = larvae[0].GetComponent<LarvaStateMachine>();
            if (stateMachine && stateMachine.CurrentState != null)
                currentStateText.text = $"Current State: {FormatStateName(stateMachine.CurrentState.StateName)}";
        }
    }
}