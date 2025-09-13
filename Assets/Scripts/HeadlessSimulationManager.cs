using UnityEngine;
using UnityEngine.Rendering;

public class HeadlessSimulationManager : MonoBehaviour
{
    [Header("Headless Configuration")]
    [SerializeField] private bool forceHeadlessMode = false;
    [SerializeField] private bool disableAllRendering = true;
    [SerializeField] private bool disableAudio = true;
    [SerializeField] private int targetFrameRate = 60;
    
    private void Awake()
    {
        var config = ConfigReader.LoadConfig();
        bool isHeadlessMode = config.headlessMode || Application.isBatchMode || forceHeadlessMode;
        
        if (isHeadlessMode)
        {
            ConfigureForHeadless();
        }
    }
    
    private void ConfigureForHeadless()
    {
        Debug.Log("Configuring simulation for headless mode...");
        
        QualitySettings.vSyncCount = 0;
        
        Application.targetFrameRate = targetFrameRate;
        
        Application.runInBackground = true;
        
        if (disableAllRendering)
        {
            Camera[] cameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in cameras)
            {
                if (cam.name != "CaptureCamera")
                {
                    cam.enabled = false;
                }
            }
            
            Renderer[] renderers = FindObjectsOfType<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = false;
            }
            
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas canvas in canvases)
            {
                canvas.enabled = false;
            }
            
            Volume[] volumes = FindObjectsOfType<Volume>();
            foreach (Volume volume in volumes)
            {
                volume.enabled = false;
            }
        }
    }
    
    private void Start()
    {
        if (Application.isBatchMode)
        {
            Debug.Log("Running in batch mode - graphics disabled by Unity");
        }
        
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Data Path: {Application.dataPath}");
        Debug.Log($"Persistent Data Path: {Application.persistentDataPath}");
        Debug.Log($"Batch Mode: {Application.isBatchMode}");
        Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
    }
}
