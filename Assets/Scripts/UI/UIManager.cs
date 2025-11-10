using System;
using Events.UICloseOpenAction;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Zenject;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text simulationTimeText;
        [SerializeField] private Slider simulationSpeedSlider;

        [SerializeField]
        private GameObject selectDrugCanvas;

        [SerializeField]
        private GameObject drugsInfoCanvas;

        [SerializeField]
        private GameObject timeCanvas;

        [SerializeField]
        private GameObject stateCanvas;

        public UnityEvent<float> onSimulationSpeedChanged = new();

        private bool _isUIOpen = true;
        private float _simulationSpeed = 1f;

        [Inject]
        private UICloseOpenActionChannel _uiCloseOpenActionChannel;

        private void Update()
        {
            simulationTimeText.SetText(
                $"{Time.timeSinceLevelLoad:F2}s, {_simulationSpeed:F2}x");
        }

        private void OnEnable()
        {
            simulationSpeedSlider.onValueChanged.AddListener(OnSimulationSpeedSliderChanged);
            _uiCloseOpenActionChannel.Register(HandleUICloseOpenAction);
        }

        private void OnDisable()
        {
            simulationSpeedSlider.onValueChanged.RemoveListener(OnSimulationSpeedSliderChanged);
            _uiCloseOpenActionChannel.Unregister(HandleUICloseOpenAction);
        }

        private void HandleUICloseOpenAction(UICloseOpenActionData data)
        {
            switch (data.windowType)
            {
                case WindowType.All:
                    switch (data.action)
                    {
                        case ActionType.Close:
                            CloseUI();
                            break;
                        case ActionType.Open:
                            ShowUI();
                            break;
                        case ActionType.Reverse:
                            if (_isUIOpen)
                                CloseUI();
                            else
                                ShowUI();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void CloseUI()
        {
            selectDrugCanvas.SetActive(false);
            drugsInfoCanvas.SetActive(false);
            timeCanvas.SetActive(false);
            stateCanvas.SetActive(false);
            _isUIOpen = false;
        }

        private void ShowUI()
        {
            selectDrugCanvas.SetActive(true);
            drugsInfoCanvas.SetActive(true);
            timeCanvas.SetActive(true);
            stateCanvas.SetActive(true);
            _isUIOpen = true;
        }

        private void OnSimulationSpeedSliderChanged(float newValue)
        {
            _simulationSpeed = Mathf.Pow(newValue, 2);

            onSimulationSpeedChanged.Invoke(_simulationSpeed);
        }
    }
}