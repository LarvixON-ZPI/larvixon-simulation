using Events.Signal;
using UnityEngine;
using Zenject;

namespace Flutter
{
    public class MainReceiver : MonoBehaviour, IFlutterReceiver
    {
        private SignalEventChannel _requestPauseEventChannel;
        private SignalEventChannel _requestResumeEventChannel;
        private SignalEventChannel _requestQuitEventChannel;
        
        [Inject]
        public void Construct(
            [Inject(Id = GameSignalId.RequestPause)] SignalEventChannel requestPauseEventChannel, 
            [Inject(Id = GameSignalId.RequestResume)] SignalEventChannel requestResumeEventChannel, 
            [Inject(Id = GameSignalId.RequestQuit)] SignalEventChannel requestQuitEventChannel)
        {
            _requestPauseEventChannel = requestPauseEventChannel;
            _requestResumeEventChannel = requestResumeEventChannel;
            _requestQuitEventChannel = requestQuitEventChannel;
        }
        
        public void HandleWebFnCall(string action)
        {
            Debug.Log($"Received action from Flutter: {action}");
            
            switch (action)
            {
                case "pause":
                    _requestPauseEventChannel.RaiseEvent();
                    break;
                case "resume":
                    _requestResumeEventChannel.RaiseEvent();
                    break;
                case "unload":
                    Application.Unload();
                    break;
                case "quit":
                    _requestQuitEventChannel.RaiseEvent();
                    break;
                default:
                    Debug.LogWarning($"Unknown action: {action}");
                    break;
            }
        }
    }
}