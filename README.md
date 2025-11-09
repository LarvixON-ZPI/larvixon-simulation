<!-- markdownlint-disable MD041 -->
[![Build Project](https://github.com/LarvixON-ZPI/larvixon-simulation/actions/workflows/build.yml/badge.svg)](https://github.com/LarvixON-ZPI/larvixon-simulation/actions/workflows/build.yml)
[![Release from Artifacts](https://github.com/LarvixON-ZPI/larvixon-simulation/actions/workflows/release-from-artifacts.yml/badge.svg)](https://github.com/LarvixON-ZPI/larvixon-simulation/actions/workflows/release-from-artifacts.yml)

# Larvixon Simulation

A Unity project simulating larvae movement with realistic peristaltic motion.

## Overview

This simulation models larvae as 5-point segmented creatures that move using peristaltic waves (contractions and extensions) similar to real larvae. Each larva consists of:

- **Head**: The front segment
- **2/5 Point**: First body segment  
- **Middle**: Center segment
- **4/5 Point**: Second body segment
- **Back**: Tail segment

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

## Build for Flutter

Make sure you’ve cloned the [LarvixON frontend repository](https://github.com/LarvixON-ZPI/larvixon-frontend) — it contains the Flutter app that includes this repo as a submodule.  
This is the easiest way to build.

1. Open the Unity project from that submodule location.  
2. In the top menu, select: **Flutter → Export WebGL**.

## Act

You need to populate **.github/workflows/.secrets** file with actual values

<https://game.ci/docs/github/activation>

```sh
act --workflows ".github/workflows/build.yml" \
--secret-file .github/workflows/.secrets \
--secret UNITY_LICENSE="$(cat ~/.local/share/unity3d/Unity/Unity_lic.ulf)"
```
