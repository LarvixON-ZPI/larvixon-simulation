using Events;
using Events.Signal;
using UnityEngine;
using Zenject;

namespace Main
{
    public class GameManager : MonoBehaviour
    {
        private SignalEventChannel _requestPauseEventChannel;
        private SignalEventChannel _requestResumeEventChannel;
        private SignalEventChannel _requestQuitEventChannel;
        private SignalEventChannel _onPausedEventChannel;
        private SignalEventChannel _onResumedEventChannel;

        private bool _isPaused;
        private float _previousTimeScale = 1f;

        [Inject]
        public void Construct(
            [Inject(Id = GameSignalId.RequestPause)] SignalEventChannel requestPause,
            [Inject(Id = GameSignalId.RequestResume)] SignalEventChannel requestResume,
            [Inject(Id = GameSignalId.RequestQuit)] SignalEventChannel requestQuit,
            [Inject(Id = GameSignalId.OnPaused)] SignalEventChannel onPaused,
            [Inject(Id = GameSignalId.OnResumed)] SignalEventChannel onResumed)
        {
            _requestPauseEventChannel = requestPause;
            _requestResumeEventChannel = requestResume;
            _requestQuitEventChannel = requestQuit;
            _onPausedEventChannel = onPaused;
            _onResumedEventChannel = onResumed;
        }

        private void OnEnable()
        {
            _requestPauseEventChannel?.RegisterListener(Pause);
            _requestResumeEventChannel?.RegisterListener(Resume);
            _requestQuitEventChannel?.RegisterListener(Quit);
        }

        private void OnDisable()
        {
            _requestPauseEventChannel?.UnregisterListener(Pause);
            _requestResumeEventChannel?.UnregisterListener(Resume);
            _requestQuitEventChannel?.UnregisterListener(Quit);
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
            _onPausedEventChannel?.RaiseEvent();
        }

        private void Resume()
        {
            if (!_isPaused) return;
            _isPaused = false;
            Time.timeScale = _previousTimeScale;
            _onResumedEventChannel?.RaiseEvent();
        }
    }
}