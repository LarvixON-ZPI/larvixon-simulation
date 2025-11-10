using Events.Signal;
using Events.UICloseOpenAction;
using UnityEngine;
using Zenject;

namespace Flutter
{
    public class MainReceiver : MonoBehaviour, IFlutterReceiver
    {
        [Inject]
        private UICloseOpenActionChannel _closeOpenActionChannel;

        [Inject(Id = GameSignalId.RequestPause)]
        private SignalEventChannel _requestPauseEventChannel;

        [Inject(Id = GameSignalId.RequestRestart)]
        private SignalEventChannel _requestRestartEventChannel;

        [Inject(Id = GameSignalId.RequestResume)]
        private SignalEventChannel _requestResumeEventChannel;

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
                case "ui":
                    _closeOpenActionChannel.Raise(new UICloseOpenActionData
                    {
                        action = ActionType.Reverse,
                        windowType = WindowType.All
                    });
                    break;
                default:
                    Debug.LogWarning($"Unknown action: {action}");
                    break;
            }
        }
    }
}