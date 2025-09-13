using System;
using System.Collections.Generic;
using System.IO;
using Drugs;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ConfigReader
{
    public static SimulationConfig LoadConfig()
    {
        return LoadConfigFromFile();
    }

    private static SimulationConfig LoadConfigFromFile()
    {
        var configPath = GetConfigPath();

        if (!File.Exists(configPath))
        {
            Debug.LogWarning($"Config file not found at {configPath}, using default values");
            return SimulationConfig.Default;
        }

        try
        {
            var configJson = File.ReadAllText(configPath);
            var config = JsonUtility.FromJson<SimulationConfig>(configJson);
            Debug.Log($"Config loaded from: {configPath}");
            return config;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load config file: {e.Message}. Using default values.");
            return SimulationConfig.Default;
        }
    }

    private static string GetConfigPath()
    {
        // In builds, look for config.json next to the executable
        var executablePath = Environment.GetCommandLineArgs()[0];
        var executableDir = Path.GetDirectoryName(executablePath);

        if (executableDir == null) throw new Exception("Could not determine executable directory");

        var buildConfigPath = Path.Combine(executableDir, "config.json");

        if (File.Exists(buildConfigPath)) return buildConfigPath;

        // In editor, look in project root
        var projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(projectRoot, "config.json");
    }

    public static void LogConfig(SimulationConfig config)
    {
        Debug.Log("Current Simulation Configuration:\n" + config);
    }

    [Serializable]
    public struct SimulationConfig
    {
        public List<DrugType> allowedDrugs;
        public float simulationTimeSeconds;
        public float simulationSpeed;
        public float framesPerSecond;
        public float intensity;
        public bool useRandomIntensity;
        public float minIntensity;
        public float maxIntensity;
        public bool headlessMode;
        public string outputPath;

        public static SimulationConfig Default => new()
        {
            simulationTimeSeconds = 600f,
            simulationSpeed = 1f,
            framesPerSecond = .25f,
            intensity = 1f,
            useRandomIntensity = false,
            minIntensity = 0.1f,
            maxIntensity = 2f,
            headlessMode = true,
            outputPath = "Recordings"
        };

        public float GetIntensity()
        {
            return useRandomIntensity ? Random.Range(minIntensity, maxIntensity) : intensity;
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this, true);
        }
    }
}