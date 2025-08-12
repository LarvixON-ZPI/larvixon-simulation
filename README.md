# Larvixon Simulation

A Unity project simulating larvae movement with realistic peristaltic motion.

## Overview

This simulation models larvae as 5-point segmented creatures that move using peristaltic waves (contractions and extensions) similar to real larvae. Each larva consists of:

- **Head**: The front segment (red)
- **2/5 Point**: First body segment  
- **Middle**: Center segment
- **4/5 Point**: Second body segment
- **Back**: Tail segment (blue)

## How It Works

### Larvae Movement Physics

- **Segmented Body**: Each larva has 5 connected points that form the body structure
- **Peristaltic Motion**: Movement is achieved through waves of contraction and extension along the body
- **Center of Mass**: Larvae change their center of mass by contracting/extending segments
- **Realistic Constraints**: Segments maintain natural lengths with spring-like forces

### Key Features

- **Wave Propagation**: Contraction waves travel from head to tail
- **Natural Locomotion**: No direct position manipulation - movement emerges from segment interactions
- **Customizable Parameters**: Adjust wave speed, contraction strength, segment length, etc.
- **Visual Feedback**: Real-time visualization with color-coded segments

## Quick Start

### Method 1: Automatic Setup

1. Create an empty GameObject in your scene
2. Add the `QuickLarvaSetup` component
3. Check "Setup Simulation" in the inspector
4. Play the scene - larvae will spawn automatically!

### Method 2: Manual Setup

1. Create an empty GameObject and add the `LarvaSimulation` component
2. Configure spawn settings (larva count, spawn area)
3. Play the scene

## Controls

- **SPACE**: Toggle auto-movement on/off
- **R**: Randomize all larvae directions

## Scripts Overview

### Core Scripts

- **`Larva.cs`**: Main larvae physics and movement logic
- **`LarvaSimulation.cs`**: Manages multiple larvae and simulation settings
- **`LarvaRenderer.cs`**: Visual rendering using LineRenderer
- **`QuickLarvaSetup.cs`**: Utility for quick scene setup

### Key Parameters

#### Larva Movement

- `segmentLength`: Distance between body segments
- `contractionStrength`: How strongly larvae push forward
- `waveSpeed`: Speed of the peristaltic wave
- `restoreForce`: Force that maintains segment constraints

#### Simulation

- `larvaCount`: Number of larvae to spawn
- `spawnArea`: Area where larvae are randomly placed
- `directionChangeInterval`: How often larvae change direction

## Future Extensions

This foundation supports adding:

- **Drug Effects**: Modify movement parameters based on different substances
- **Environmental Interactions**: Response to temperature, light, obstacles
- **Behavioral Patterns**: Feeding, clustering, avoidance behaviors
- **Data Collection**: Track movement patterns, speed, efficiency metrics

## Technical Details

The simulation uses a constraint-based physics approach:

1. **Wave Generation**: Sine waves create rhythmic segment length targets
2. **Constraint Forces**: Springs maintain segment relationships
3. **Emergent Movement**: Forward motion emerges from coordinated contractions
4. **Stability**: Damping prevents oscillations and maintains smooth movement

Each larva operates independently, allowing for complex collective behaviors to emerge naturally.
