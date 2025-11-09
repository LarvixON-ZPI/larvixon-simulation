# Larvixon Simulation Configuration

This file contains the configuration for the Larvixon simulation. Place this file in the same directory as the executable for builds.

## Configuration Options

- **allowedDrugs**: specify allowed drugs to be simulated by providing full names as a comma/space/semicolon separated list
  - to specify that all are allowed: "*"
  - following list example would allow for these 5 (currently all available drugs): "cOCAine ; morPHINE,tetrodotoxin, ethanol;ketamine"
- **simulationTimeSeconds**: Duration of the simulation in seconds
- **simulationSpeed**: Speed multiplier for the simulation
- **framesPerSecond**: How many times per second take a screenshot
- **intensity**: Drug intensity value (must be between 0.0 and 1.0)
- **useRandomIntensity**: Whether to use random intensity between min/max values
- **minIntensity**: Minimum intensity for random mode
- **maxIntensity**: Maximum intensity for random mode
- **outputPath**: Directory for output recordings (default: "Recordings")
  - Can be an absolute path (e.g., "/tmp/recordings" on Linux/macOS, "C:\\temp\\recordings" on Windows)
  - Can be a relative path (relative to the executable in builds, or project root in editor)
  - Supports tilde expansion on Unix-like systems (e.g., "~/recordings", "~/Documents/simulations")
- **resolutionWidth**: Width of the output resolution (default: 960)
- **resolutionHeight**: Height of the output resolution
- **videoFormat**: Format of the output video
  - Supported formats: "mp4", "avi", "mov", "webm"

## Usage

1. **For builds**: Place `config.json` in the same directory as the executable
2. **For development**: The config file is located in the project root directory

## Standard configuration

```json
{
  "allowedDrugs": "*",
  "simulationTimeSeconds": 600.0,
  "simulationSpeed": 4.0,
  "framesPerSecond": 4.0,
  "intensity": 1.0,
  "useRandomIntensity": false,
  "minIntensity": 0.1,
  "maxIntensity": 1.0,
  "outputPath": "Recordings",
  "resolutionWidth": 960,
  "resolutionHeight": 540,
  "videoFormat": "mp4"
}
```
