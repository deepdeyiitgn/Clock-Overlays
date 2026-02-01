# Transparent Clock

Transparent Clock is a lightweight, always‑on‑top clock overlay for Windows with a simple dashboard and pomodoro controls.

Version: Transparent Clock v01.02.2026

## Key Features
- Transparent clock overlay with color, size, and position controls
- Pomodoro timer (work / short break / long break)
- Dashboard for clock, pomodoro, profile, and settings
- Profile and settings stored locally
- 100% offline (no tracking, no internet)

## Privacy
This app is fully offline. It does not send data, use tracking, or require internet access.

## How to Run
From the repository root:

dotnet run --project src/TransparentClock.csproj

## Build (Single EXE)
Build a single Windows EXE:

dotnet publish src/TransparentClock.csproj -c Release -r win-x64

Output EXE:
src/bin/Release/net8.0-windows/win-x64/publish/TransparentClock.exe

## Local Data
On first run, the app creates its data folder automatically at:
%LOCALAPPDATA%\Clock-Overlays\appstate.json

## Desktop Shortcut
After publishing, create a shortcut to the EXE (manual or via installer). The EXE already embeds the app icon.

## Author
Deep Dey
