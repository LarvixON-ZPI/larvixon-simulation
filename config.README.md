# Larvixon Simulation Configuration

This file contains the configuration for the Larvixon simulation. Place this file in the same directory as the executable for builds.

## Configuration Options

- **simulationTimeSeconds**: Duration of the simulation in seconds (default: 600)
- **simulationSpeed**: Speed multiplier for the simulation (default: 1.0)
- **framesPerSecond**: How many times per second take a screenshot
- **intensity**: Drug intensity value (default: 1.0, must be between 0.0 and 1.0)
- **useRandomIntensity**: Whether to use random intensity between min/max values (default: false)
- **minIntensity**: Minimum intensity for random mode (default: 0.1)
- **maxIntensity**: Maximum intensity for random mode (default: 1.0)
- **headlessMode**: Whether to run without graphics (default: true)
- **outputPath**: Directory for output recordings (default: "Recordings")

## Usage

1. **For builds**: Place `config.json` in the same directory as the executable
2. **For development**: The config file is located in the project root directory

## Standard configuration

```json
{
  "simulationTimeSeconds": 600.0,
  "simulationSpeed": 1.0,
  "framesPerSecond": 0.25,
  "intensity": 1.0,
  "useRandomIntensity": false,
  "minIntensity": 0.1,
  "maxIntensity": 1.0,
  "headlessMode": true,
  "outputPath": "Recordings"
}
```
