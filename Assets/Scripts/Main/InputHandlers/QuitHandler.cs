using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main.InputHandlers
{
    public class QuitHandler : MonoBehaviour
    {
        private SignalEventChannel _requestQuitEventChannel;
        
        [Inject]
        public void Construct(
            [Inject(Id = GameSignalId.RequestQuit)] SignalEventChannel requestResume)
        {
            _requestQuitEventChannel = requestResume;
        }

        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
                _requestQuitEventChannel.RaiseEvent();
        }
    }
}