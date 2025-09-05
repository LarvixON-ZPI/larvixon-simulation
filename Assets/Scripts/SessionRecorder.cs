using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Drugs;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Random = System.Random;

// ReSharper disable NotAccessedField.Local

public class SessionRecorder : MonoBehaviour
{
    [Header("References")] public LarvaSimulation larvaSimulation;
    public Camera captureCamera;

    [Header("Recording Settings")] public int sessionLengthSeconds = 600;
    public string outputRootFolder = "Recordings";
    public float captureFps = 30;
    public bool useUnityRecorderIfAvailable = true;
    public bool useFfmpegFallback = true;
    public bool generateVideo = true;

    [Header("JSON Settings")] public bool prettyPrintJson;
    private readonly List<FrameData> _frameBuffer = new();
    private IReadOnlyList<DrugEffect> _availableDrugs;
    private int _currentFrameIndex;
    private string _currentFramesFolder;
    private string _currentJsonPath;
    private float _currentSessionDosage;
    private DrugEffect _currentSessionDrug;
    private string _currentSessionFolder;
    private float _currentSessionSimulationStartTime;
    private string _currentSessionStartIso;

    private float _frameInterval;
    private bool _isRecording;
    private Random _rng;

    private RenderTexture _rt;
    private Texture2D _screenShotTexture;
    private float _sessionElapsed;
    private float _timeSinceLastFrame;

    private void Awake()
    {
        _rng = new Random();
        if (captureFps <= 0)
            throw new ArgumentOutOfRangeException(nameof(captureFps), "Capture FPS must be greater than zero.");
        _frameInterval = 1f / captureFps;

        _rt = new RenderTexture(Screen.width, Screen.height, 24);
        _screenShotTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
    }

    private void Start()
    {
        _availableDrugs = larvaSimulation?.GetAvailableDrugEffects();

        BeginSession();
    }

    private void Update()
    {
        if (!_isRecording || !larvaSimulation) return;

        var dt = Time.deltaTime;
        _sessionElapsed += dt;
        _timeSinceLastFrame += dt;

        if (_timeSinceLastFrame >= _frameInterval)
        {
            _timeSinceLastFrame -= _frameInterval;
            CaptureFrame();
        }

        if (_sessionElapsed >= sessionLengthSeconds) EndCurrentSession().Forget();
    }

    private void BeginSession()
    {
        _sessionElapsed = 0f;
        _timeSinceLastFrame = 0f;
        _currentFrameIndex = 0;
        _frameBuffer.Clear();

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _currentSessionFolder = Path.Combine(Application.dataPath, "..", outputRootFolder, $"session_{timestamp}");
        Directory.CreateDirectory(_currentSessionFolder);
        _currentFramesFolder = Path.Combine(_currentSessionFolder, "frames");
        Directory.CreateDirectory(_currentFramesFolder);
        _currentJsonPath = Path.Combine(_currentSessionFolder, "larva_points.json");

        _currentSessionStartIso = DateTime.Now.ToString("o");
        _currentSessionSimulationStartTime = Time.time;

        ApplyRandomDrugAtFullDose();

        _isRecording = true;
    }

    private void ApplyRandomDrugAtFullDose()
    {
        if (_availableDrugs == null || _availableDrugs.Count == 0)
        {
            Debug.LogWarning("No drug effects available to apply.");
            return;
        }

        larvaSimulation.ClearAllDrugsFromLarvae();

        var index = _rng.Next(_availableDrugs.Count);
        var chosen = _availableDrugs[index];
        _currentSessionDrug = chosen;
        _currentSessionDosage = 1f;
        larvaSimulation.ApplyDrugToAllLarvaeWithDosage(chosen, _currentSessionDosage);

        Debug.Log($"SessionRecorder: Started session with drug: {chosen.drugName}");
    }

    private void CaptureFrame()
    {
        var larvae = larvaSimulation.Larvae;
        var frame = new FrameData
        {
            frameIndex = _currentFrameIndex,
            timestamp = Time.time,
            larvae = new List<LarvaData>(larvae.Count)
        };

        for (var i = 0; i < larvae.Count; i++)
        {
            var larva = larvae[i];
            var pointsCopy = new Vector2[larva.points.Length];
            Array.Copy(larva.points, pointsCopy, pointsCopy.Length);
            frame.larvae.Add(new LarvaData
            {
                larvaIndex = i,
                points = pointsCopy
            });
        }

        _frameBuffer.Add(frame);

        CapturePngFrame();

        _currentFrameIndex++;
    }

    private void CapturePngFrame()
    {
        if (!captureCamera) return;

        captureCamera.targetTexture = _rt;
        captureCamera.Render();
        RenderTexture.active = _rt;
        _screenShotTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        _screenShotTexture.Apply();
        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        var bytes = _screenShotTexture.EncodeToPNG();

        var fileName = Path.Combine(_currentFramesFolder, $"frame_{_currentFrameIndex:D06}.png");
        File.WriteAllBytes(fileName, bytes);
    }

    private async UniTaskVoid EndCurrentSession()
    {
        if (!_isRecording) return;
        _isRecording = false;

        try
        {
            WriteBufferedJsonToDisk();
            if (generateVideo) await TryAssembleVideoAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"SessionRecorder: Error finalizing session: {ex}");
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void WriteBufferedJsonToDisk()
    {
        var simulationEndTime = Time.time;
        var session = new SessionData
        {
            sessionDrugName = _currentSessionDrug ? _currentSessionDrug.drugName : string.Empty,
            sessionDosage = _currentSessionDosage,
            sessionStartTimeIso = _currentSessionStartIso,
            simulationStartTime = _currentSessionSimulationStartTime,
            simulationEndTime = simulationEndTime,
            captureFps = captureFps,
            frameCount = _frameBuffer.Count,
            frames = _frameBuffer
        };

        var json = JsonUtility.ToJson(session, prettyPrintJson);
        File.WriteAllText(_currentJsonPath, json, Encoding.UTF8);
    }

    private async UniTask TryAssembleVideoAsync()
    {
        if (useUnityRecorderIfAvailable)
        {
        }

        if (!useFfmpegFallback) return;

        const string ffmpegPath = "ffmpeg";
        var sessionVideoPath = Path.Combine(_currentSessionFolder, "session.mp4");

        // Build a ffmpeg command to convert PNG sequence to MP4 at captureFps
        var args =
            $"-y -framerate {captureFps} -i \"{Path.Combine(_currentFramesFolder, "frame_%06d.png")}\" -c:v libx264 -pix_fmt yuv420p -movflags +faststart \"{sessionVideoPath}\"";

        await RunShellCommandAsync(ffmpegPath, args);
    }

    private static async UniTask RunShellCommandAsync(string fileName, string arguments)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            if (proc == null) return;
            await UniTask.WaitUntil(() => proc.HasExited);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"SessionRecorder: Failed to run command '{fileName} {arguments}'. {e.Message}");
        }
    }

    [Serializable]
    private class SessionData
    {
        public string sessionDrugName;
        public float sessionDosage;
        public string sessionStartTimeIso;
        public float simulationStartTime;
        public float simulationEndTime;
        public float captureFps;
        public int frameCount;
        public List<FrameData> frames;
    }

    [Serializable]
    private class FrameData
    {
        public int frameIndex;
        public float timestamp;
        public List<LarvaData> larvae;
    }

    [Serializable]
    private class LarvaData
    {
        public int larvaIndex;
        public Vector2[] points;
    }
}