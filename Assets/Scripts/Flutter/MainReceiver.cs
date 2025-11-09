using Events.Signal;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Flutter
{
    public class MainReceiver : MonoBehaviour, IFlutterReceiver
    {
        [Inject(Id = GameSignalId.RequestPause)] 
        private SignalEventChannel _requestPauseEventChannel;
        [Inject(Id = GameSignalId.RequestResume)] 
        private SignalEventChannel _requestResumeEventChannel;
        [Inject(Id = GameSignalId.RequestRestart)] 
        private SignalEventChannel _requestRestartEventChannel;
        
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
                case "restart":
                    _requestRestartEventChannel.Raise();
                    break;
                default:
                    Debug.LogWarning($"Unknown action: {action}");
                    break;
            }
        }
    }
}