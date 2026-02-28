# V-Proofix: AI-Powered Grammar & Style Assistant

<p align="center">
  <img src="Resources/logo.png" width="128" alt="V-Proofix Logo" />
</p>

V-Proofix is a professional Windows desktop application designed to streamline your writing and proofreading workflow. By leveraging advanced Large Language Models (LLMs) from Google Gemini, the app provides instant grammar correction, spelling fixes, and stylistic enhancements directly within any text editor or browser.

---

## Key Features

*   **Multilingual AI Proofreading:** Powered by Google Gemini (optimized for **Gemini 2.0 Flash**) to analyze context and deliver lightning-fast, precise linguistic improvements.
*   **Seamless System Integration:** Operates via system-wide hotkeys, allowing you to process text in any application (Word, Browser, IDE, Slack, etc.) without manual copy-pasting.
*   **Liquid Glass Interface:** A modern UI featuring high-end Glassmorphism effects, ensuring a premium aesthetic that blends beautifully with your workspace.
*   **Smart Preview System:** Provides an intuitive side-by-side comparison (Diff) between original and corrected text before applying changes.
*   **Enterprise-Grade Security:** API keys are encrypted using the Windows Data Protection API (DPAPI), ensuring your sensitive data remains local and secure.
*   **History Tracking:** Locally stores previous proofreading sessions for easy reference and retrieval.

---

## System Hotkeys

V-Proofix supports two primary operation modes with customizable shortcuts:

| Mode | Default Hotkey | Description |
| :--- | :--- | :--- |
| **Fix Now** | `Ctrl + Alt + F` | Automatically corrects errors and overwrites the selected text directly. |
| **Preview Fix** | `Ctrl + Alt + P` | Opens a side-by-side comparison window for manual verification before applying. |

---

## Prerequisites & Installation

### System Requirements
*   **OS:** Windows 10/11 (64-bit).
*   **Runtime:** .NET 8.0 Desktop Runtime.
*   **Connection:** Active Internet connection for Google Gemini API access.

### Getting Started
1.  Download the latest release from the [Releases](https://github.com/rainaku/V-Proofix/releases) page.
2.  Extract the package and run `VProofix.exe`.
3.  Access **Settings** from the System Tray icon.
4.  Configure your **Gemini API Key** (Get a free one at [Google AI Studio](https://aistudio.google.com/)).

---

## Technical Architecture

V-Proofix is built with a focus on stability, performance, and modern standards:

*   **Language:** C# / XAML (WPF).
*   **Framework:** .NET 8.0.
*   **AI Integration:** Google Gemini API via REST.
*   **Automation:** Windows UI Automation & Win32 API for seamless inter-process interaction.
*   **Security:** DPAPI (System.Security.Cryptography.ProtectedData) for local credential encryption.

---

## License

This project is licensed under the **MIT License**. For more details, please see the `LICENSE` file in the root directory.

---

## Contact & Contribution

Contributions are welcome to help improve the tool!
*   **Bug Reports & Feature Requests:** [GitHub Issues](https://github.com/rainaku/V-Proofix/issues).
*   **Author:** **rainaku**

