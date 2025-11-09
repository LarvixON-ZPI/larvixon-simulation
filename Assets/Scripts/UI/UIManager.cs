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
        private GameObject drugCanvas;

        [SerializeField]
        private GameObject timeCanvas;

        [SerializeField]
        private GameObject stateCanvas;

        public UnityEvent<float> onSimulationSpeedChanged = new();
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
                            drugCanvas.SetActive(false);
                            timeCanvas.SetActive(true);
                            stateCanvas.SetActive(true);
                            break;
                        case ActionType.Open:
                            drugCanvas.SetActive(true);
                            timeCanvas.SetActive(false);
                            stateCanvas.SetActive(false);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    return;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnSimulationSpeedSliderChanged(float newValue)
        {
            _simulationSpeed = Mathf.Pow(newValue, 2);

            onSimulationSpeedChanged.Invoke(_simulationSpeed);
        }
    }
}