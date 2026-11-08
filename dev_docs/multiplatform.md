# Multiplatform

> **Status: to do.** Written on 2026-11-08, when it turned out that the window specifications
> (`JAStudio.UI.Views.Specifications`) can run only on Windows, and Magnus pointed out that JAStudio is explicitly meant
> to be multiplatform. Nothing here has been tried on macOS or Linux. The inventory below is from reading the code.

## The goal

JAStudio is meant to run on **Windows, macOS and Linux**, as Anki does (Magnus, 2026-11-08). We want that unless
achieving it would really slow progress: it's a goal, not non-negotiable.

Until 2026-11-08 the goal was written down only in `text-to-speech.md` ("JAStudio isn't Windows-only"). Since then
the project instructions say it too (`.claude/rules/02-universal-local/020-project-orientation.md`).

## What ties JAStudio to Windows (inventory of 2026-11-08)

| What | Where | Why it's there | The way out |
|---|---|---|---|
| `GetCursorPos` from Windows' `user32` | `WindowPositioner`: `ShowNearCursor`, `RepositionNearCursor`, `CursorPosition` | Opens windows and popup menus at the mouse cursor. It started as a way to place Avalonia windows opened from Anki's Qt UI, which Avalonia knows nothing of. | Magnus, 2026-11-08: multiplatform support outweighs opening at the cursor. See [Placing windows without the cursor](#placing-windows-without-the-cursor). |
| `SetFocus` from Windows' `user32` | `NativeKeyboardFocus` | The web view and Avalonia's controls competing for the keyboard. On Windows the web view is a native window of its own, which keeps Windows' keyboard focus when Avalonia's focus moves to a control. So the window takes it back: when Ctrl+O puts the keyboard on the search, when an Avalonia control in the workspace is clicked, when the workspace is activated again (written while Windows' keyboard input was broken system-wide, so perhaps not needed), and when a dialog opens over the workspace (`DialogInitialFocus`, `ConfirmDialog`). | Find out whether the macOS and Linux web views have the same problem. Then a seam with an implementation per platform, or a fix or report upstream in Avalonia. Headless windows have no native window, so specifications of the dialogs need the seam too. |
| WebView2 settings | `NoteTabView.ConfigureWebView2Environment` | Keeps the web view's profile out of Anki's install folder, and lets a page play audio without a click first. | Already applied on Windows only (it checks the event's type). Find out what WKWebView (macOS) and WebKitGTK (Linux) need instead, if anything. |
| Windows-only Japanese fonts | `JapaneseFont` in `ImportRunWindow`, `TextToSpeechDialog`, `SearchResultsView`, and inline in `ListedNotesView`: Meiryo UI, Yu Gothic UI, Microsoft YaHei UI | The Japanese text's look in the Avalonia UI. Elsewhere, Japanese still shows, in whatever font the system falls back to. | Easy: add macOS's (Hiragino Sans) and Linux's (Noto Sans CJK JP) to the list, which skips fonts that aren't installed. Best as one definition in `App.axaml`, which also ends the per-view definitions whose absence crashed Anki once. The cards' CSS already lists Hiragino. |
| The window specifications skip non-Windows | `IsTestProject` in `JAStudio.UI.Views.Specifications.csproj` | The windows they open call `GetCursorPos`, so CI, which runs on Linux, skips them. | Remove the condition once `GetCursorPos` is gone, and see whether headless Skia runs in CI. |

## Placing windows without the cursor

Discussed with Magnus on 2026-11-08; not decided in detail.
- **Context menus in Avalonia views** (search tree, search results, tag management): the context-menu event carries
  the pointer's position, which `PointToScreen` turns into a screen position. No OS call is needed.
- **The note page's context menu** in a workspace tab: Avalonia doesn't see the pointer over the native web view, but
  the page's right-click event knows where it happened. It can send that along, converted for the page's zoom.
- **The generic context menu Anki asks for** (`AnkiMenus.ShowContextMenuPopup`) already gets its position from Qt,
  through Python. Nothing to change.
- **Windows opened from another JAStudio window:** centred over that window, by Avalonia.
- **Windows opened from Anki:** hardly any are needed any more (Magnus, 2026-11-08). Anki's "Japanese" menu and its
  shortcuts can go: the workspace shows the same menu, `JapaneseMainMenu`, as its menu bar, with the same shortcuts.
  The reviewer's hotkeys open no windows.
- **The workspace** is the one window opened from Anki, by its toggle. It can remember its place between sessions
  (Magnus), unless Avalonia's own placement turns out fine. A remembered place must be checked against the screens
  connected now, since a monitor may be gone.
- **The reviewer's right-click menu,** and the note page's menu in the workspace, which is the same menu
  (`NoteContextMenu`), are to go (Magnus, 2026-11-08). They were a hack to get some UI for a rendered note before the
  cards were rendered with Blazor, and they guess what was clicked from the text JavaScript found selected. A
  Blazor UI on the page itself, which knows what was clicked, is to replace them.

## Not known yet

- **Whether the add-on runs in Anki on macOS or Linux at all:** pythonnet loading .NET 10, and where the .NET
  runtime comes from.
- **Avalonia's UI thread inside Anki on macOS.** JAStudio runs Avalonia on a thread of its own (`AvaloniaUIThread` in
  `JAStudioAnkiAppRoot`), while Qt has Anki's main thread. macOS requires UI to run on the main thread, so this may be
  the hardest part.
- **Two GUI toolkits in one process on Linux:** Qt, and Avalonia with WebKitGTK.
- **Native libraries per platform.** `runtime_binaries` is copied from the newest local build
  (`copy_runtime_binaries.py`), built for the build machine's runtime identifier. SkiaSharp, HarfBuzz, SoundFlow's
  audio and the web view have native parts per platform, so other platforms need builds of their own.
- **Developer tooling:** `full-build.ps1` is PowerShell, which runs on all three, but uses `venv\Scripts` paths.
  `setup-dev.sh` already sets up Linux, and CI runs on Linux.

## Already multiplatform

- **The Python layer:** no Windows-specific code found.
- **Audio:** SoundFlow plays it (`text-to-speech.md`). Mp3 normalization is plain C#.
- **The card server:** Kestrel.
- **The web view:** Avalonia's `NativeWebView` has backends for WebView2, WKWebView and WebKitGTK.
- **Settings and data folders** come from `Environment.SpecialFolder` and `IEnvironmentPaths`.
- **The STA apartment** for Avalonia's thread is set on Windows only already (`OperatingSystem.IsWindows`).
