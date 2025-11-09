using Drugs;
using Events.ApplyDrug;
using Events.MoveLarvaToPoint;
using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main
{
    public class Debugger : MonoBehaviour
    {
        [Inject(Id = GameSignalId.RequestPause)]
        private SignalEventChannel _requestPauseEventChannel;
        [Inject(Id = GameSignalId.RequestResume)]
        private SignalEventChannel _requestResumeEventChannel;
        [Inject(Id = GameSignalId.ClearDrugs)]
        private SignalEventChannel _clearDrugsEventChannel;
        
        [Inject]
        private ApplyDrugEventChannel _applyDrugEventChannel;
        [Inject]
        private SetDestinationForLarva _setDestinationForLarva;
        
        private Camera _camera;

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