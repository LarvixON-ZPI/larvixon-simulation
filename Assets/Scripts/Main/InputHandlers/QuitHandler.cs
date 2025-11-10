using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main.InputHandlers
{
    public class QuitHandler : MonoBehaviour
    {
        [Inject(Id = GameSignalId.RequestQuit)]
        private SignalEventChannel _requestQuitEventChannel;
        
        private void Update()
        {
            HandleInput();
        }

        private void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) 
                _requestQuitEventChannel.Raise();
        }
    }
}