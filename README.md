# Transparent Clock / Clock Overlays

**v05.02.2026 (BETA)** — Professional Windows Desktop Productivity Tool

A lightweight, always-on-top clock overlay with advanced focus tracking, productivity dashboard, and integrated utilities. Pure local storage, zero cloud tracking, 100% user privacy.

> ⚠️ **BETA Release**: Features and behavior may change. Report issues on [GitHub](https://github.com/deepdeyiitgn/Clock-Overlays/issues).

## Overview

Transparent Clock combines a persistent overlay clock with a powerful companion dashboard. Track daily focus patterns, manage tasks, run Pomodoro cycles, and scan/generate QR codes—all stored locally on your machine.

**Perfect for**: Students, developers, remote workers, focus-seeking professionals, and anyone tracking productivity.

## Implemented Features

### Core Features
- **Transparent Clock Overlay** — Always-on-top system clock with custom font, size, color, and position saving
- **Dashboard Interface** — Tabbed control center with all features accessible in one window
- **Persistent Settings** — All preferences stored locally (JSON files); nothing sent to cloud

### Focus & Productivity
- **Focus Tracking System** — 24-hour auto-tracked focus duration per session with intelligent session detection
- **Focus History & Analytics** — 7-day rolling history with time range selectors (Last 7/30 Days, 6/12 Months)
- **Smart Statistics** — Average daily focus, best/worst focus slots, daily distribution analysis
- **Focus Graphs** — Line graphs showing focus trends + 24-hour heatmaps for pattern recognition
- **Heatmap Visualization** — Daily focus heatmap (beta, in-progress; cell interaction coming soon)
- **Pomodoro System** — Work, short break, and long break cycles with auto-cycle progression and sound notifications
- **To-Do List** — Full task management with date range filtering, completion tracking, bulk operations

### Utilities Suite (Beta)
- **QR Code Generator** — Create 13+ QR code types (URL, WiFi, Email, vCard, Calendar, UPI, Phone, SMS, WhatsApp, Location, Cryptocurrency, Bluetooth, and more)
- **QR Export Options** — PNG, JPG, SVG, PDF, HEIC formats with embedded QuickLink logo and metadata
- **QR Code Scanner** — Scan saved QR images with drag-drop support, automatic type detection, and payload preview
- **QuickLink URL Shortener** — Generate short URLs with API key management, result caching, and error handling
- **Smart QR Detection** — Auto-detects QR type and extracts structured data (WiFi credentials, contact info, etc.)

### System & Privacy
- **100% Local Storage** — All data stays on your machine (settings, focus history, to-do items)
- **Zero Tracking** — No telemetry, analytics, or external data collection
- **Logo Caching** — QuickLink logo downloaded once and cached locally for offline operation
- **Version Awareness** — Built-in update notifications for new beta releases

## Installation (Windows)

### For End Users (Recommended)
1. Download the latest installer: [TransparentClock-v05.02.2026-Beta-Setup.exe](https://github.com/deepdeyiitgn/Clock-Overlays/releases/tag/v05.02.2026)
2. Run the installer
3. Select installation directory
4. Optionally create desktop shortcut
5. Launch application

**System Requirements**: Windows 10/11, .NET 8 Runtime (included in installer)

### For Developers
Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

```bash
git clone https://github.com/deepdeyiitgn/Clock-Overlays.git
cd Clock-Overlays/src
dotnet run
```

## System Requirements
- **OS**: Windows 10 / 11 (x64)
- **RAM**: 4 GB recommended
- **Storage**: ~500 MB (self-contained .NET 8 runtime included)
- **Framework**: .NET 8 (bundled in installer)

## Local Data & Privacy
All user data stored locally at:
```
%LOCALAPPDATA%\Clock-Overlays\
  ├── appstate.json
  ├── todos.json
  ├── focus_history.json
  ├── focus_sessions.json
  └── quicklink_api_key.txt (if using URL shortener)
```

✅ **No cloud sync**  
✅ **No tracking or telemetry**  
✅ **No internet required** (except optional QuickLink API for URL shortening)  
✅ **Full user control**

## Beta Notice

**Transparent Clock is currently in BETA.** Features and interfaces may change as the product evolves. We welcome feedback and bug reports via [GitHub Issues](https://github.com/deepdeyiitgn/Clock-Overlays/issues).

**Known Limitations**:
- Heatmap visualization still in development
- Some focus graph edge cases may cause display artifacts
- QR code customization (gradient, eye styles, logo selection) deferred to stable release

**Supported Platforms**: Windows 10 (21H2+), Windows 11

## Contributing & Support

- **Issues**: [Report bugs or request features](https://github.com/deepdeyiitgn/Clock-Overlays/issues)
- **Discussions**: [Community feedback](https://github.com/deepdeyiitgn/Clock-Overlays/discussions)
- **License**: MIT (see [LICENSE](LICENSE))

---

## Development

Built with:
- **Language**: C# 12
- **Framework**: .NET 8.0 WinForms
- **Libraries**: QRCoder, ZXing.Net, SkiaSharp, PdfSharp, ImageSharp

## Author

**Deep Dey** — Created January 2026

---

### 📸 Some Screenshots Below: 

<img width="288" height="209" alt="Screenshot 2026-02-02 202627" src="https://github.com/user-attachments/assets/ee2f565f-8c11-4a61-aa6f-0eedbd08f933" />
<img width="1942" height="327" alt="Screenshot 2026-02-02 202606" src="https://github.com/user-attachments/assets/198112e5-52c2-439c-a725-645ffe6055e7" />
<img width="1943" height="369" alt="Screenshot 2026-02-02 202600" src="https://github.com/user-attachments/assets/3f36c66e-4b43-4bff-9aa5-873150e1c8ab" />
<img width="1943" height="956" alt="Screenshot 2026-02-02 202553" src="https://github.com/user-attachments/assets/71185c69-70b8-4c83-b47d-9631042855cf" />
<img width="1943" height="595" alt="Screenshot 2026-02-02 202546" src="https://github.com/user-attachments/assets/b6fb0a77-6f17-4493-a769-0c635399e7fd" />
<img width="1943" height="1103" alt="Screenshot 2026-01-29 182932" src="https://github.com/user-attachments/assets/3a6b399b-77f5-425c-b959-84243f84b899" />
<img width="246" height="291" alt="Screenshot 2026-01-29 182650" src="https://github.com/user-attachments/assets/b0261434-c31a-4a8f-b1bc-bdb4070ad7dc" />
<img width="506" height="416" alt="Screenshot 2026-01-29 182638" src="https://github.com/user-attachments/assets/186d9a10-979c-4ab4-826f-153a22c095a7" />

---

## Author
Deep Dey
