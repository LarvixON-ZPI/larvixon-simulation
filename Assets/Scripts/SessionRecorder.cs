using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Drugs;
using UnityEngine;
using UnityEngine.Rendering;
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
    public float captureFps = 0.25f;
    public bool useFfmpegFallback = true;
    public bool generateVideo = true;
    public float simulationSpeed = 1f;
    public float dosage = 1f;
    
    [Header("Config Mode")]
    public bool enableConfigMode = true;
    private ConfigReader.SimulationConfig _config;

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
    private float? _deathTime;

    private float _frameInterval;
    private bool _isRecording;
    private Random _rng;

    private RenderTexture _rt;
    private Texture2D _screenShotTexture;
    private float _sessionElapsed;
    private float _timeSinceLastFrame;

    private void Awake()
    {
        if (enableConfigMode) LoadConfig();
        
        _rng = new Random();
        if (captureFps <= 0)
            throw new ArgumentOutOfRangeException(nameof(captureFps), "Capture FPS must be greater than zero.");
        _frameInterval = 1f / captureFps;

        var w = Screen.width / 2;
        var h = Screen.height / 2;
        _rt = new RenderTexture(w, h, 1);
        _screenShotTexture = new Texture2D(w, h, TextureFormat.RGB24, false);
    }

    private void LoadConfig()
    {
        _config = ConfigReader.LoadConfig();
        
        sessionLengthSeconds = (int)_config.simulationTimeSeconds;
        simulationSpeed = _config.simulationSpeed;
        dosage = _config.intensity;
        outputRootFolder = _config.outputPath;
        
        ConfigReader.LogConfig(_config);
        
        if (_config.headlessMode)
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            generateVideo = false; 
            captureFps = 0;
        }
    }

    private void Start()
    {
        _availableDrugs = larvaSimulation?.GetAvailableDrugEffects();

        larvaSimulation!.OnSimulationSpeedChanged(simulationSpeed);

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

        if (_deathTime == null && !larvaSimulation.Larvae[0].IsAlive) _deathTime = _sessionElapsed;

        if (_sessionElapsed >= sessionLengthSeconds) EndCurrentSession().Forget();
    }

    private void BeginSession()
    {
        _sessionElapsed = 0f;
        _timeSinceLastFrame = Random.Range(0f, _frameInterval);
        _currentFrameIndex = 0;
        _frameBuffer.Clear();

        string drugName;
        if (enableConfigMode && _config.useRandomIntensity)
        {
            var newIntensity = UnityEngine.Random.Range(_config.minIntensity, _config.maxIntensity);
            dosage = newIntensity;
        }
        
        drugName = ApplyRandomDrugAtDose(dosage);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        _currentSessionFolder =
            Path.Combine(Application.dataPath, "..", outputRootFolder, $"{drugName}_{dosage:F2}_{timestamp}");
        Directory.CreateDirectory(_currentSessionFolder);
        
        _currentFramesFolder = Path.Combine(_currentSessionFolder, "frames");
        Directory.CreateDirectory(_currentFramesFolder);
        
        _currentJsonPath = Path.Combine(_currentSessionFolder, "larva_points.json");

        _currentSessionStartIso = DateTime.Now.ToString("o");
        _currentSessionSimulationStartTime = _sessionElapsed;

        _isRecording = true;
    }

    private string ApplyRandomDrugAtDose(float appliedDosage)
    {
        _currentSessionDosage = appliedDosage;
            
        var index = _rng.Next(_availableDrugs.Count);
        var chosen = _availableDrugs[index];
        _currentSessionDrug = chosen;
        larvaSimulation.ApplyDrugToAllLarvaeWithDosage(chosen, appliedDosage);

        Debug.Log($"SessionRecorder: Started session with drug: {chosen.drugName}");

        return chosen.drugName;
    }

    private void CaptureFrame()
    {
        var larvae = larvaSimulation.Larvae;
        var frame = new FrameData
        {
            frameIndex = _currentFrameIndex,
            timestamp = _sessionElapsed,
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
        if (!captureCamera || _rt == null) return;

        captureCamera.targetTexture = _rt;
        captureCamera.Render();
        captureCamera.targetTexture = null;

        AsyncGPUReadback.Request(_rt, 0, TextureFormat.RGB24, request =>
        {
            if (request.hasError) return;
            var data = request.GetData<byte>().ToArray();
            var tex = new Texture2D(_rt.width, _rt.height, TextureFormat.RGB24, false);
            tex.LoadRawTextureData(data);
            tex.Apply();
            var bytes = tex.EncodeToPNG();
            Destroy(tex);

            if (!string.IsNullOrEmpty(_currentFramesFolder))
            {
                var fileName = Path.Combine(_currentFramesFolder, $"frame_{_currentFrameIndex:D06}.png");
                File.WriteAllBytesAsync(fileName, bytes);
            }
        });
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
        var simulationEndTime = _sessionElapsed;
        var session = new SessionData
        {
            sessionDrugName = _currentSessionDrug ? _currentSessionDrug.drugName : string.Empty,
            sessionDosage = _currentSessionDosage,
            sessionStartTimeIso = _currentSessionStartIso,
            simulationStartTime = _currentSessionSimulationStartTime,
            simulationEndTime = simulationEndTime,
            captureFps = captureFps,
            frameCount = _frameBuffer.Count,
            deathTime = _deathTime ?? -1f,
            frames = _frameBuffer
        };

        var json = JsonUtility.ToJson(session, prettyPrintJson);
        File.WriteAllText(_currentJsonPath, json, Encoding.UTF8);
    }

    private async UniTask TryAssembleVideoAsync()
    {
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
        public float deathTime;
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