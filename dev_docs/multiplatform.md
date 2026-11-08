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
| `GetCursorPos` from Windows' `user32` | `MouseCursor`, for the note page's right-click menu in a workspace tab | Avalonia doesn't see the pointer over the page's native web view. | Goes with the menu, which a Blazor UI on the page is to replace. *Until 2026-11-08 every window and popup menu opened at the cursor: see [Placing windows without the cursor](#placing-windows-without-the-cursor).* |
| `SetFocus` from Windows' `user32` | `NativeKeyboardFocus` | The web view and Avalonia's controls competing for the keyboard. On Windows the web view is a native window of its own, which keeps Windows' keyboard focus when Avalonia's focus moves to a control. So the window takes it back: when Ctrl+O puts the keyboard on the search, when an Avalonia control in the workspace is clicked, when the workspace is activated again (written while Windows' keyboard input was broken system-wide, so perhaps not needed), and when a dialog opens over the workspace (`DialogInitialFocus`, `ConfirmDialog`). | Find out whether the macOS and Linux web views have the same problem. Then a seam with an implementation per platform, or a fix or report upstream in Avalonia. Headless windows have no native window, so specifications of the dialogs need the seam too. |
| WebView2 settings | `NoteTabView.ConfigureWebView2Environment` | Keeps the web view's profile out of Anki's install folder, and lets a page play audio without a click first. | Already applied on Windows only (it checks the event's type). Find out what WKWebView (macOS) and WebKitGTK (Linux) need instead, if anything. |

## Placing windows without the cursor

*Done 2026-11-08,* but for the note page's menu, as decided with Magnus that day:
- **Context menus in Avalonia views** (search tree, search results, tag management): the context-menu event carries
  the pointer's position, which `PointToScreen` turns into a screen position. No OS call is needed.
- **The note page's context menu** in a workspace tab still opens at the cursor (`MouseCursor`): it's to be replaced
  (below), so it isn't worth reworking.
- **The generic context menu Anki asks for** (`AnkiMenus.ShowContextMenuPopup`) already gets its position from Qt,
  through Python. Nothing to change.
- **Windows open where they were last** (`WindowPlaceExtensions`, `RememberedWindowPlaces`), the workspace included.
  A window remembers its place when another window is activated or it's closed, in `window-places.json` in the user
  files folder. The first time, and when its place is on no connected screen, Avalonia centres it on the screen.
  Transient windows, the confirm dialog and the progress dialog, are always centred on the screen.
- **Windows opened from Anki:** hardly any are needed any more (Magnus). Anki's "Japanese" menu is gone: the
  workspace shows the same menu, now `WorkspaceMainMenu`, as its menu bar, with the same shortcuts. Anki keeps
  Ctrl+Shift+O, which toggles the workspace, and Ctrl+O, which opens it on its search. The reviewer's hotkeys open no
  windows.
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

- **Placing windows:** see above.
- **The Japanese fonts** in the Avalonia UI: one `JapaneseFont` in `App.axaml`, listing Windows', macOS's (Hiragino
  Sans) and Linux's (Noto Sans CJK JP). Fonts that aren't installed are skipped.
- **The window specifications** run on every platform. *Whether headless Skia works in CI on Linux shows on the next
  push.*

- **The Python layer:** no Windows-specific code found.
- **Audio:** SoundFlow plays it (`text-to-speech.md`). Mp3 normalization is plain C#.
- **The card server:** Kestrel.
- **The web view:** Avalonia's `NativeWebView` has backends for WebView2, WKWebView and WebKitGTK.
- **Settings and data folders** come from `Environment.SpecialFolder` and `IEnvironmentPaths`.
- **The STA apartment** for Avalonia's thread is set on Windows only already (`OperatingSystem.IsWindows`).
