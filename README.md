# Transparent Clock

Transparent Clock is a lightweight, always‑on‑top clock overlay for Windows with a clean dashboard and Pomodoro controls.

Version: v02.02.2026 (BETA)

## App Overview
Transparent Clock keeps a minimal overlay clock visible while you work. A companion dashboard lets you adjust appearance, position, and Pomodoro sessions with local‑only settings.

## Features
- Always‑on‑top transparent clock overlay
- Font family, size, color, and position controls
- Custom position adjustment and persistence
- Pomodoro timer (work / short break / long break)
- Profile and settings stored locally
- 100% offline (no tracking, no internet)

## System Requirements
- Windows 10 / 11
- x64 CPU
- 4 GB RAM recommended
- ~1 GB storage
- .NET 8 included (self‑contained build)

## Beta Warning
This is a beta release. Expect occasional UI quirks or edge‑case bugs.

## Open‑Source Notice
Transparent Clock is open‑source. You are free to inspect, modify, and distribute under the project license.

## How to Run
From the repository root:

dotnet run --project src/TransparentClock.csproj

## Local Data
On first run, the app creates its data file at:
%LOCALAPPDATA%\Clock-Overlays\appstate.json

## Roadmap (Short)
- Accessibility and keyboard shortcuts
- More clock styles and themes
- Pomodoro analytics and history

## Author
Deep Dey
