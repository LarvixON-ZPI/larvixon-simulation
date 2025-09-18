# Larvixon Simulation

A Unity project simulating larvae movement with realistic peristaltic motion.

## Overview

This simulation models larvae as 5-point segmented creatures that move using peristaltic waves (contractions and extensions) similar to real larvae. Each larva consists of:

- **Head**: The front segment (red)
- **2/5 Point**: First body segment  
- **Middle**: Center segment
- **4/5 Point**: Second body segment
- **Back**: Tail segment (blue)

## Drugs

Apply a drug to the currently selected larva by pressing its key:

- m — Morphine (reduces movement, slows peristaltic waves)
- c — Cocaine (increases activity; faster, more frequent contractions)
- e — Ethanol (depressant; reduces coordination, weakens contractions)
- k — Ketamine (dissociative; disrupts normal wave patterns)
- t — Tetrodotoxin (TTX; blocks neural activity, halts movement)

## Running recorder

You need to have ffmpeg on path

## Recorder json

If deathTime is -1, then larva has not died

## Logs location

Windows standalone:

```text
C:\Users\<User>\AppData\LocalLow\Larvixon\simulation\Player.log
```

Linux standalone:

```text
~/.config/unity3d/Larvixon/simulation/Player.log
```

Mac standalone:

```text
~/Library/Logs/Larvixon/simulation/Player.log
```

## Builds

- Script: `tools/build.sh` builds Windows and Linux players via Unity CLI and copies `README.md` and `config.README.md` into each build folder.
- Requires `UNITY_PATH` to point to your Unity Editor binary.

Quick usage (Linux host):

```sh
chmod +x tools/build.sh
cp tools/.env.example tools/.env
```

edit tools/.env to set UNITY_PATH, etc.

```sh
./tools/build.sh
```
