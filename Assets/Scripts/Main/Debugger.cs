using Drugs;
using Events.ApplyDrug;
using Events.MoveLarvaToPoint;
using Events.Signal;
using Events.UICloseOpenAction;
using UnityEngine;
using Zenject;

namespace Main
{
    public class Debugger : MonoBehaviour
    {
        [Inject]
        private ApplyDrugEventChannel _applyDrugEventChannel;

        private Camera _camera;

        [Inject(Id = GameSignalId.ClearDrugs)]
        private SignalEventChannel _clearDrugsEventChannel;

        [Inject]
        private UICloseOpenActionChannel _closeOpenActionChannel;

        [Inject(Id = GameSignalId.RequestPause)]
        private SignalEventChannel _requestPauseEventChannel;

        [Inject(Id = GameSignalId.RequestResume)]
        private SignalEventChannel _requestResumeEventChannel;

        [Inject]
        private SetDestinationForLarva _setDestinationForLarva;

        public void Start()
        {
            _camera = Camera.main;
#if !UNITY_EDITOR
            Destroy(this);
#endif
        }

        public void Update()
        {
            HandleInput();
        }

        private void ApplyDrug(DrugType drugType)
        {
            _applyDrugEventChannel.Raise(new ApplyDrugData
            {
                drugType = drugType,
                intensity = DrugSystem.MaxDrugIntensity
            });
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.P))
                _requestPauseEventChannel.Raise();

            if (Input.GetKeyDown(KeyCode.R))
                _requestResumeEventChannel.Raise();

            if (Input.GetKeyDown(KeyCode.H))
                _closeOpenActionChannel.Raise(new UICloseOpenActionData
                {
                    action = ActionType.Reverse,
                    windowType = WindowType.All
                });

            // Drug testing controls
            if (Input.GetKeyDown(KeyCode.C)) ApplyDrug(DrugType.Cocaine);
            if (Input.GetKeyDown(KeyCode.E)) ApplyDrug(DrugType.Ethanol);
            if (Input.GetKeyDown(KeyCode.T)) ApplyDrug(DrugType.Tetrodotoxin);
            if (Input.GetKeyDown(KeyCode.K)) ApplyDrug(DrugType.Ketamine);
            if (Input.GetKeyDown(KeyCode.M)) ApplyDrug(DrugType.Morphine);

            if (Input.GetKeyDown(KeyCode.X))
            {
                _clearDrugsEventChannel.Raise();
                Debug.Log("Cleared all drugs from all larvae");
            }

            if (Input.GetMouseButton(1))
            {
                var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                _setDestinationForLarva.Raise(mouseWorldPos);
            }
        }
    }
}