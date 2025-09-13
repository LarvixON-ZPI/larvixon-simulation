# Larvixon Simulation Configuration

This file contains the configuration for the Larvixon simulation. Place this file in the same directory as the executable for builds.

## Configuration Options

- **allowedDrugs**: specify allowed drugs to be simulated by providing full names as a comma/space/semicolon separated list
  - to specify that all are allowed: "*"
  - following list example would allow for these 5 (currently all available drugs): "cOCAine ; morPHINE,tetrodotoxin, ethanol;ketamine"
- **simulationTimeSeconds**: Duration of the simulation in seconds (default: 600)
- **simulationSpeed**: Speed multiplier for the simulation (default: 1.0)
- **framesPerSecond**: How many times per second take a screenshot
- **intensity**: Drug intensity value (default: 1.0, must be between 0.0 and 1.0)
- **useRandomIntensity**: Whether to use random intensity between min/max values (default: false)
- **minIntensity**: Minimum intensity for random mode (default: 0.1)
- **maxIntensity**: Maximum intensity for random mode (default: 1.0)
- **headlessMode**: Whether to run without graphics (default: true)
- **outputPath**: Directory for output recordings (default: "Recordings")
  - Can be an absolute path (e.g., "/tmp/recordings" on Linux/macOS, "C:\\temp\\recordings" on Windows)
  - Can be a relative path (relative to the executable in builds, or project root in editor)
  - Supports tilde expansion on Unix-like systems (e.g., "~/recordings", "~/Documents/simulations")

## Usage

1. **For builds**: Place `config.json` in the same directory as the executable
2. **For development**: The config file is located in the project root directory

## Standard configuration

```json
{
  "allowedDrugs": "*",
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
