using Events.Signal;
using UnityEngine;
using Zenject;

namespace Flutter
{
    public class MainReceiver : MonoBehaviour, IFlutterReceiver
    {
        [Inject(Id = GameSignalId.RequestPause)] 
        private SignalEventChannel _requestPauseEventChannel;
        [Inject(Id = GameSignalId.RequestResume)] 
        private SignalEventChannel _requestResumeEventChannel;
        [Inject(Id = GameSignalId.RequestQuit)] 
        private SignalEventChannel _requestQuitEventChannel;
        
        public void HandleWebFnCall(string action)
        {
            Debug.Log($"Received action from Flutter: {action}");
            
            switch (action)
            {
                case "pause":
                    _requestPauseEventChannel.Raise();
                    break;
                case "resume":
                    _requestResumeEventChannel.Raise();
                    break;
                case "unload":
                    Application.Unload();
                    break;
                case "quit":
                    _requestQuitEventChannel.Raise();
                    break;
                default:
                    Debug.LogWarning($"Unknown action: {action}");
                    break;
            }
        }
    }
}