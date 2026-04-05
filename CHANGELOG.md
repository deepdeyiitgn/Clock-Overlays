# Changelog

## v08.02.2026 (Stable)
- Finalized Pomodoro system with accurate focus tracking
- Stable QR Generator and Scanner
- Utilities tab finalized
- UI polish and consistency improvements
- Bug fixes and performance stability

## v05.02.2026 – Beta — 05 Feb 2026

### Added
- **Focus Insights System** — Time range selector (Last 7/30 Days, 6/12 Months) with intelligent statistics cards
- **Focus Analytics** — Average focus per day, best/worst focus slot detection with non-zero filtering
- **QR Code Generator** — Support for 13 QR types (URL, WiFi, Email, vCard, Calendar, UPI, Phone, SMS, WhatsApp, Location, Cryptocurrency, Bluetooth, and more)
- **QR Code Generator Enhancements** — Multi-format export (PNG, JPG, SVG, PDF, HEIC) with embedded QuickLink logo
- **QR Code Scanner** — Smart QR scanning with automatic type detection, structured data parsing, and drag-drop support
- **QR Scanner Online API** — Network-aware QR decoding with clear error messaging for network status
- **QuickLink URL Shortener** — URL shortening with API key management, result caching, and comprehensive error handling
- **Utilities Tab** — Complete QR tools and URL shortener in one integrated interface
- **Logo Caching System** — One-time QuickLink logo download with local cache for offline operation
- **Focus Graphs Foundation** — Line graphs and 24-hour bar charts with date picker controls
- **Heatmap System** — Daily focus heatmap (beta, in-progress)

### Improved
- **To-Do System** — Advanced filtering by date range and completion status, bulk delete operations
- **To-Do Visuals** — Gray text and strikethrough styling for completed tasks
- **Focus History** — Fixed calculation logic for worst-slot detection (excludes zero-minute slots)
- **Error Handling** — Comprehensive try-catch blocks and null safety across all new features
- **User Messaging** — Professional error messages with network status awareness
- **QuickLink API** — Robust JSON parsing with Accept header enforcement and fallback endpoints
- **QR Scanning Reliability** — Transitioned to online API for better QR type support

### Fixed
- Focus history calculation edge cases with empty date ranges
- To-Do filter persistence and refresh timing
- Focus statistics accuracy for time range calculations
- QR generator payload encoding for all QR types
- QuickLink API JSON parse errors with response validation
- QR scanner image handling and network error messaging

### Known Issues
- Heatmap cell interaction and date filtering still in development
- QR code gradient and eye-style customization deferred to v06
- HEIC export falls back to JPEG (system codec limitation)
- Some systems may experience brief lag when loading QuickLink logo for first time
- QR scanning requires internet connection for online API mode

### Breaking Changes
- None

---

## v04.02.2026 – Beta — Previous Release

### Added
- Version awareness (local metadata + update prompt).
- 7-day focus history + 24-hour heatmap insights.
- To-Do workflow improvements and stability polish.

### Fixed
- Pomodoro stability and session completion tracking.
- Miscellaneous UI and persistence bug fixes.

## v03.02.2026 – Beta

### Added
- Focus history tracking (local JSON) and Insights tab.
- 7-day focus overview and daily focus map.
- Local To-Do system with date range and filters.

### Known Issues
- UI polish pending across new dashboard sections.
- Windows-only API warnings during build (expected on non-Windows analysis).

## v02.02.2026 – Beta

### Added
- Font family selection for the clock overlay (system fonts).
- Auto-start breaks toggle in Pomodoro settings.
- Custom clock position saved per user.
- Inno Setup installer script.

### Fixed
- Welcome screen now appears only on true first run.
- Pomodoro layout alignment and control clipping issues.
- Clock overlay top-most stability and dynamic resizing.
- Clock overlay clipping at large font sizes.

### Known Issues
- Windows-only API warnings during build (expected on non-Windows analysis).
- Some systems may require elevated permissions for startup tasks.

## v01.02.2026
- Initial stable release
- Clock overlay with customization
- Pomodoro with work / break cycles
- Dashboard-based control
- Profile & settings stored locally
- No cloud, no ads, no tracking
