using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main
{
    public class GameManager : MonoBehaviour
    {
        [Inject(Id = GameSignalId.RequestPause)]
        private SignalEventChannel _requestPauseEventChannel;
        [Inject(Id = GameSignalId.RequestResume)]
        private SignalEventChannel _requestResumeEventChannel;
        [Inject(Id = GameSignalId.RequestQuit)]
        private SignalEventChannel _requestQuitEventChannel;
        [Inject(Id = GameSignalId.OnPaused)]
        private SignalEventChannel _onPausedEventChannel;
        [Inject(Id = GameSignalId.OnResumed)]
        private SignalEventChannel _onResumedEventChannel;

        private bool _isPaused;
        private float _previousTimeScale = 1f;

        private void OnEnable()
        {
            _requestPauseEventChannel?.Register(Pause);
            _requestResumeEventChannel?.Register(Resume);
            _requestQuitEventChannel?.Register(Quit);
        }

        private void OnDisable()
        {
            _requestPauseEventChannel.Unregister(Pause);
            _requestResumeEventChannel?.Unregister(Resume);
            _requestQuitEventChannel?.Unregister(Quit);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Pause()
        {
            if (_isPaused) return;
            _isPaused = true;
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _onPausedEventChannel?.Raise();
        }

        private void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = _previousTimeScale;
            _onResumedEventChannel?.Raise();
        }
    }
}