using System;
using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main
{
    public class Debugger : MonoBehaviour
    {
        private SignalEventChannel _requestPauseEventChannel;
        private SignalEventChannel _requestResumeEventChannel;
        
        [Inject]
        public void Construct(
            [Inject(Id = GameSignalId.RequestPause)] SignalEventChannel requestPause,
            [Inject(Id = GameSignalId.RequestResume)] SignalEventChannel requestResume)
        {
            _requestPauseEventChannel = requestPause;
            _requestResumeEventChannel = requestResume;
        }

        public void Start()
        {
#if !UNITY_EDITOR
            Destroy(this);
#endif
        }

        public void Update()
        {
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.P))
                _requestPauseEventChannel.RaiseEvent();
            
            if (Input.GetKeyDown(KeyCode.R))
                _requestResumeEventChannel.RaiseEvent();

            // // Drug testing controls
            // if (Input.GetKeyDown(KeyCode.C)) ApplyDrugToAllLarvae(cocaineEffect);
            // if (Input.GetKeyDown(KeyCode.E)) ApplyDrugToAllLarvae(ethanolEffect);
            // if (Input.GetKeyDown(KeyCode.T)) ApplyDrugToAllLarvae(tetrodotoxinEffect);
            // if (Input.GetKeyDown(KeyCode.K)) ApplyDrugToAllLarvae(ketamineEffect);
            // if (Input.GetKeyDown(KeyCode.M)) ApplyDrugToAllLarvae(morphineEffect);
            //
            // if (Input.GetKeyDown(KeyCode.X))
            // {
            //     ClearAllDrugsFromLarvae();
            //     Debug.Log("Cleared all drugs from all larvae");
            // }
            //
            // if (Input.GetMouseButton(1))
            // {
            //     var mouseWorldPos = _camera.ScreenToWorldPoint(Input.mousePosition);
            //     mouseWorldPos.z = 0;
            //
            //     foreach (var larva in _larvae)
            //     {
            //         var larvaPos = larva.GetCenter();
            //         var directionToMouse = ((Vector2)mouseWorldPos - larvaPos).normalized;
            //         larva.SetMovementDirection(directionToMouse);
            //     }
            // }
        }
    }
}