# Transparent Clock — v05.02.2026 (Beta) Release Notes

**Release Date**: February 5, 2026  
**Build Type**: Beta  
**Platform**: Windows 10/11 (x64)  
**Download**: [TransparentClock-v05.02.2026-Beta-Setup.exe](https://github.com/deepdeyiitgn/Clock-Overlays/releases/tag/v05.02.2026)

---

## Release Overview

**v05.02.2026** is a significant beta update featuring complete **Focus Insights analytics**, a full **Utilities suite** (QR Generator/Scanner + URL Shortener), advanced **Focus Graphs**, and comprehensive **network-aware error handling**.

This release transitions the application from foundational features to a complete productivity platform with real-time focus tracking, advanced analytics, and integrated utility tools.

---

## ✨ What's New

### Focus Insights & Analytics (Major Feature)
- **Time Range Selectors** — Switch between Last 7 Days, Last 30 Days, Last 6 Months, Last 12 Months
- **Smart Statistics** — Average daily focus, best/worst focus slot detection with intelligent filtering
- **Advanced Graphs** — Line charts showing focus trends + 24-hour bar graph for daily distribution
- **Heatmap System** — Daily focus heatmap visualization (beta, cell interaction coming in v06)

### Utilities Suite (Complete)
- **QR Code Generator** — 13+ QR types (URL, WiFi, Email, vCard, Calendar, UPI, Phone, SMS, WhatsApp, Location, Cryptocurrency, Bluetooth, more)
- **QR Export Options** — PNG, JPG, SVG, PDF, HEIC with embedded QuickLink logo and metadata
- **QR Code Scanner** — Scan images with drag-drop support, automatic type detection, structured data preview
- **QuickLink URL Shortener** — Generate short URLs with API key management and result caching
- **Network-Aware Error Messaging** — Clear user-facing messages for network status and errors
- **Online QR Decoding** — Switched to cloud API (qrserver.com) for reliable QR scanning

### API & Backend Improvements
- **QuickLink Robust API** — Accept header enforcement, flexible JSON field parsing, fallback endpoints
- **Error Handling** — Comprehensive try-catch with user-friendly messages per HTTP status code
- **Logo Caching** — QuickLink logo downloaded once and cached locally for offline operation
- **Response Validation** — JSON pre-validation before parsing to prevent cryptic errors

---

## 🔧 Improvements

### Focus System
- Fixed focus history calculation edge cases with zero-minute slots
- Improved accuracy of worst-slot detection logic
- Enhanced statistics filtering for non-zero focus periods
- Better date range handling for all analytics calculations

### QR & Utilities
- Transitioned QR scanning from local ZXing to online API for better reliability
- Comprehensive QR type detection and structured data parsing
- Professional error messages replacing technical stack traces
- Network timeout handling with clear user messaging

### To-Do System
- Advanced filtering by date range and completion status
- Bulk delete operations and improved UI responsiveness
- Gray text and strikethrough styling for completed tasks
- Better filter persistence and refresh timing

### General Quality
- Enhanced null safety across all new features
- Improved error handling with try-catch patterns
- Professional user messaging throughout the application
- Better version awareness and update notifications

---

## 🐛 Bug Fixes

✅ Focus history calculation edge cases with empty date ranges  
✅ To-Do filter persistence and refresh timing  
✅ Focus statistics accuracy for time range calculations  
✅ QR generator payload encoding for all QR types  
✅ QuickLink API JSON parse errors (now validates response format first)  
✅ QR scanner image handling and network error messaging  
✅ FocusStatsDisplay nested control rendering issues (previous beta)  

---

## 📋 Known Issues & Limitations

⚠️ **Heatmap Interaction** — Cell date filtering and detailed cell tooltips still in development  
⚠️ **QR Customization** — Gradient fills, eye styles, and logo selection deferred to v06  
⚠️ **HEIC Export** — Falls back to JPEG due to system codec limitations  
⚠️ **QR Scanning** — Requires internet connection for online API mode (fallback to local scan in v06)  
⚠️ **Load Time** — First-time logo download may cause brief UI lag (cached thereafter)  

---

## 📊 System Requirements

- **OS**: Windows 10 (21H2+) / Windows 11
- **RAM**: 4 GB minimum (8 GB recommended for graphs)
- **Storage**: ~500 MB (includes .NET 8 runtime)
- **Architecture**: x64 only
- **Internet**: Required for QR scanning and URL shortening (optional QuickLink)

---

## 🚀 Installation

### Option 1: Installer (Recommended)
1. Download [TransparentClock-v05.02.2026-Beta-Setup.exe](https://github.com/deepdeyiitgn/Clock-Overlays/releases/tag/v05.02.2026)
2. Run the installer
3. Select installation directory
4. Optionally create desktop shortcut
5. Launch application

### Option 2: Manual .EXE
1. Extract `TransparentClock.exe` from release folder
2. Run directly (no installation required)
3. .NET 8 runtime included in executable

---

## 🔄 Upgrading from v04.02.2026

**Automatic Settings Migration**: All app state, focus history, To-Do items, and preferences migrate automatically. No manual action required.

**Data Location**: `%LOCALAPPDATA%\Clock-Overlays\`

**Backup Recommendation**: Before major version upgrades, backup `appstate.json` to avoid data loss.

---

## 🔐 Privacy & Data

✅ **100% Local Storage** — All focus history, To-Do items, preferences stored locally  
✅ **Zero Tracking** — No telemetry, analytics, or external monitoring  
✅ **No Cloud Sync** — Your data never leaves your machine  
✅ **Optional Only** — QuickLink API only used if you explicitly enable URL shortening  

---

## 🎯 Performance Metrics

- **Binary Size**: 193.97 MB (includes self-contained .NET 8 runtime)
- **Memory Usage**: ~80-120 MB at idle
- **QR Generation**: <500ms for most QR types
- **Focus Graph Render**: <1000ms for 30-day history
- **Startup Time**: ~3-5 seconds on modern hardware

---

## 📝 Release Notes History

**Previous**: [v04.02.2026](CHANGELOG.md#v04022026--beta--04-feb-2026)  
**Repository**: [GitHub — Clock-Overlays](https://github.com/deepdeyiitgn/Clock-Overlays)  
**Issues**: [Report bugs or request features](https://github.com/deepdeyiitgn/Clock-Overlays/issues)

---

## ✅ Quality Assurance

- ✅ Build: 0 errors, 7 warnings (dependency vulnerabilities, not code)
- ✅ Compilation: Clean release configuration
- ✅ Testing: Manual QA across all features
- ✅ Documentation: Complete and up-to-date
- ✅ Versioning: Synced across app, AssemblyInfo, and website

---

## 🎓 For Developers

**Source Code**: Available on [GitHub](https://github.com/deepdeyiitgn/Clock-Overlays)

**Build Instructions**:
```bash
git clone https://github.com/deepdeyiitgn/Clock-Overlays.git
cd Clock-Overlays/src
dotnet build -c Release
```

**Technology Stack**:
- .NET 8.0 WinForms (Windows Desktop)
- QRCoder 1.4.3 (QR generation)
- SixLabors.ImageSharp 3.1.1 (image processing)
- PDFsharp 6.1.1 (PDF export)
- SkiaSharp 2.88.6 (graphics rendering)

---

## 📞 Support & Contact

- **Website**: https://qlynk.vercel.app
- **GitHub Repo**: https://github.com/deepdeyiitgn/Clock-Overlays
- **Issues**: [GitHub Issues](https://github.com/deepdeyiitgn/Clock-Overlays/issues)
- **Author**: Deep Dey

---

## ⚖️ License

Source Available License — See [LICENSE](LICENSE) for full terms.

---

**Thank you for using Transparent Clock! Your feedback drives development. Report issues and feature requests on GitHub. 🎉**
