# V-Proofix Changelogs

## Version 1.1.0

### 🎉 New Features
- **Global Input Blocker**: Implemented a comprehensive Keyboard and Mouse hook system (Low-Level API) that temporarily freezes physical input while the application is processing grammar fixes. This prevents accidental tab switches (Alt+Tab) or mouse clicks that could cause the target text box to lose focus.
- **Human-Readable AI Models**: The application now seamlessly translates backend AI model identifiers into clean, professional display names (e.g., `gemma-3-27b-it` is shown as `Gemma 3 27B`, `gemini-2.5-flash` as `Gemini 2.5 Flash`) for better aesthetics during API calls.
- **Smart Limitation System**: Added an automatic fail-safe that rejects texts exceeding **600 words**, protecting your API key from hitting rate limits due to excessive payload sizes on single requests.
- **Full Localization (i18n)**: Completely translated the application's user interface. You can now toggle between **English** and **Vietnamese** seamlessly in the Settings window without having to restart the application.

### 🎨 UI/UX Improvements
- **MacOS-Inspired Indicator Redesign**: Expanded the Progress UI window and added a sleek, modern, light-blue glow (`DropShadowEffect`) around vector icons (Spinner, Checkmark, Error Cross) and primary status text.
- **Progress Simulation Illusion**: Revamped the visual feedback loop. The application now only shows the boring "Calling API..." state for exactly 1 second. The remaining network latency is masked behind a high-speed percentage simulation (`Analyzing...` -> `Fixing X characters (Y%)...`), giving users the illusion of lightning-fast local processing.
- **Settings Dashboard Tuning**: Fixed window scaling issues. The settings dashboard height is now perfectly proportioned, and user resizing has been securely locked out to maintain strict layout integrity.
- **Simplified Messaging**: Removed redundant warning text ("Please do not switch tabs") since the new physical `InputBlocker` makes it completely unnecessary.

### ⚙️ Under the Hood
- **New Default Brain**: Switched the default processing engine to Google's highly impressive **`Gemma 3 27B`** (Google's latest 3rd Generation Open Model) instead of Gemini Flash for optimal offline-grade intelligence.
- **Robust Model Fallback Chain**: Upgraded the backend logic to gracefully degrade. If the Gemma model fails or exhausts its quota, the application will forcefully fall back through a long chain of Gemini models (`Gemini 2.5 Flash`, `Gemini 2.5 Pro`, `Gemini 2.0 Flash`, etc.) ensuring a near 100% processing success rate.
- **Rate Limit Cleanups**: Stripped out legacy, manual Requests-Per-Minute and Requests-Per-Day tracking logic due to Google API platform limitations, leading to slightly faster initialization times.
