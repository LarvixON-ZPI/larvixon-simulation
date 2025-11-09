using Events.Signal;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Main
{
    public class GameManager : MonoBehaviour
    {
        private bool _isPaused;

        [Inject(Id = GameSignalId.OnPaused)]
        private SignalEventChannel _onPausedEventChannel;

        [Inject(Id = GameSignalId.OnResumed)]
        private SignalEventChannel _onResumedEventChannel;

        private float _previousTimeScale = 1f;

        [Inject(Id = GameSignalId.RequestPause)]
        private SignalEventChannel _requestPauseEventChannel;

        [Inject(Id = GameSignalId.RequestQuit)]
        private SignalEventChannel _requestQuitEventChannel;

        [Inject(Id = GameSignalId.RequestRestart)]
        private SignalEventChannel _requestRestartEventChannel;

        [Inject(Id = GameSignalId.RequestResume)]
        private SignalEventChannel _requestResumeEventChannel;

        private void OnEnable()
        {
            _requestPauseEventChannel.Register(Pause);
            _requestResumeEventChannel.Register(Resume);
            _requestQuitEventChannel.Register(Quit);
            _requestRestartEventChannel.Register(Restart);
        }

        private void OnDisable()
        {
            _requestPauseEventChannel.Unregister(Pause);
            _requestResumeEventChannel.Unregister(Resume);
            _requestQuitEventChannel.Unregister(Quit);
            _requestRestartEventChannel.Unregister(Restart);
        }

        private static void Quit()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
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

        private static void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}