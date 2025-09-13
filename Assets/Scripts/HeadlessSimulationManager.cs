using UnityEngine;
using UnityEngine.Rendering;

public class HeadlessSimulationManager : MonoBehaviour
{
    [Header("Headless Configuration")]
    [SerializeField] private bool forceHeadlessMode;

    [SerializeField] private bool disableAllRendering = true;
    [SerializeField] private bool disableAudio = true;
    [SerializeField] private int targetFrameRate = 60;

    private void Awake()
    {
        var config = ConfigReader.LoadConfig();
        var isHeadlessMode = config.headlessMode || Application.isBatchMode || forceHeadlessMode;

        if (isHeadlessMode) ConfigureForHeadless();
    }

    private void Start()
    {
        if (Application.isBatchMode) Debug.Log("Running in batch mode - graphics disabled by Unity");

        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Data Path: {Application.dataPath}");
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
        Debug.Log($"Batch Mode: {Application.isBatchMode}");
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
    }

    private void ConfigureForHeadless()
    {
        Debug.Log("Configuring simulation for headless mode...");

        QualitySettings.vSyncCount = 0;

        Application.targetFrameRate = targetFrameRate;

        Application.runInBackground = true;

        if (!disableAllRendering) return;

        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        foreach (var cam in cameras)
            if (cam.name != "CaptureCamera")
                cam.enabled = false;

        var renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        foreach (var sceneRenderer in renderers) sceneRenderer.enabled = false;

        var canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var canvas in canvases) canvas.enabled = false;

        var volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (var volume in volumes) volume.enabled = false;
    }
}