# Workarounds

Active workarounds for upstream-tool bugs that affect this repo. Each section explains the bug, the workaround in place today, the teardown plan for when the bug is fixed, and how to recognize when the workaround has regressed.

---

## C# language server (csharp-ls)

The `csharp-lsp@claude-plugins-official` plugin has a known bug ([anthropics/claude-code#16360](https://github.com/anthropics/claude-code/issues/16360)) where Claude Code's LSP client doesn't implement the `workspace/configuration` handler csharp-ls needs to learn which solution to load. csharp-ls then falls back to heuristics and indexes the wrong `.slnx` in this repo (the Compze submodule's, not `JAStudio.slnx`).

### The ideal config (what we want once #16360 is fixed)

Each project's `.claude/settings.json` declares its solution path; Claude Code forwards it to csharp-ls via the standard LSP `workspace/configuration` channel. No user-global files. No env-var indirection. Teammates clone the repo and C# code intelligence just works.

`lspSettings` is already in our `.claude/settings.json` as a forward-compatible declaration — it's currently a no-op but documents intent.

### The workaround that's actually doing the work today

Two files, both required, both temporary:

**1. Project-level `<repo>/.claude/settings.json`** (git-tracked, in this repo):
```json
{
  "env": { "CSHARP_LSP_SOLUTION_REL": "src/src_dotnet/JAStudio.slnx" }
}
```
For another C# project, change the value to that project's solution path.

**2. User-global `~/.claude/plugins/cache/claude-plugins-official/csharp-lsp/<version>/.lsp.json`** (not in any repo — has to exist on each dev machine):
```json
{
  "csharp": {
    "command": "csharp-ls",
    "args": ["--solution", "${CLAUDE_PROJECT_DIR}/${CSHARP_LSP_SOLUTION_REL}", "--loglevel", "info"],
    "extensionToLanguage": { ".cs": "csharp", ".csx": "csharp" }
  }
}
```
This is the "bridge" — it reads the env var (set per-project by file #1) and passes it to csharp-ls via its CLI flag.

**No comments in these JSON files** — Claude Code uses strict JSON, not jsonc. `//` lines cause parse errors. Use `"_comment"` keys if you really need inline notes.

### When #16360 is fixed

1. Delete the entire user-global `.lsp.json` from step 2.
2. Delete the `env` block from each project's `.claude/settings.json`.
3. Verify `lspSettings.csharp.solutionPathOverride` (already present) takes over.

### Troubleshooting

If `documentSymbol`, `findReferences`, or `hover` on JAStudio C# code return "No symbols found" or symbols only from the Compze submodule, the user-global `.lsp.json` from step 2 is missing or malformed. Recreate it. (It lives in Claude Code's auto-managed plugin cache and may be clobbered on csharp-lsp plugin updates.)

---

## PowerShell tool in VS Code extension UI mode

The PowerShell tool fails on **every** invocation when Claude Code runs inside the VS Code extension's UI mode (the native webview — sidebar or editor tab). It returns `Exit code 1` with no stdout/stderr, even for trivially safe commands like `exit 0`, `Write-Output "hi"`, or `2+2`. Failure is immediate (~10 ms) — pwsh never actually runs the command (a `Out-File` test produces no file).

Upstream bug: [anthropics/claude-code#55671](https://github.com/anthropics/claude-code/issues/55671) (canonical, open). Duplicates: [#57311](https://github.com/anthropics/claude-code/issues/57311) (closed as dup; exact symptom match). Related variant: [#55727](https://github.com/anthropics/claude-code/issues/55727) (Japanese locale, "command line is invalid").

### Root cause

Claude Code's PowerShell permission classifier embeds the full settings context (`permissions.additionalDirectories`, allow/deny/ask lists, the MCP deferred-tools list, etc.) into the `pwsh -Command "..."` invocation. On Windows that command line can exceed the `CreateProcess` 32,767-character limit, at which point pwsh exits 1 with `The command line is too long.` before running anything. The classifier runs on every PowerShell call, so every call fails uniformly — including ones that wouldn't need permission checks (e.g., in `bypassPermissions` mode).

The Bash tool uses a different invocation path and is unaffected.

### Why this repo trips it

This session connects a large number of MCP servers (claude.ai integrations + plugin servers — 70+ deferred tools). Even though no setting in this repo or in `~/.claude/settings.json` lists `additionalDirectories`, the MCP tools list alone appears to bloat the classifier payload past the Win32 limit. (Verify: the `~/.claude.json` projects entries and both `settings.json` files have empty `allowedTools` / no `additionalDirectories` — so user config is not the source of bloat.)

### Why UI mode and not terminal mode

Empirically: same session, same settings, same machine — PowerShell tool works in **terminal mode** (`claudeCode.useTerminal: true`) and fails in **UI mode** (sidebar or editor-tab webview). The UI harness packs more wrapping/context into each pwsh classifier invocation than the terminal harness. (See the comment thread on #55671 — Magnus contributed this data point.)

### The workaround that's actually doing the work today

**Invoke pwsh via the Bash tool** instead of using the PowerShell tool directly:

```
pwsh -NoProfile -NonInteractive -Command "<your PowerShell here>"
```

Tested working in this session. Bash spawns pwsh through a path that doesn't hit the Win32 limit, and the profile-skip + non-interactive flags avoid stalling on the user's `$PROFILE` (which would otherwise emit ANSI cursor codes and `cd C:\dev`).

Use this whenever PowerShell-specific behavior is needed (e.g., `.\full-build.ps1`, `.\setup-agent.ps1`, native PS cmdlets, Windows-paths needing PS quoting). Plain shell commands should still go through Bash directly.

### Other workarounds considered and rejected

- **Switch to terminal mode** — works, but the user strongly prefers the UI (output is much more readable). Rejected for day-to-day use.
- **Disable MCP servers to shrink the classifier payload** — plausible but untested, and the user uses those servers in Chat/Cowork. Rejected unless the bug becomes blocking.
- **Roll back to extension v2.1.123** (last known-good per #55727) — costs ~20 versions of fixes. Rejected.
- **Prune `additionalDirectories` / permission lists** — already empty in this install; nothing to prune.

### When #55671 is fixed

The suggested upstream fix is to pipe the classifier payload via stdin or write it to a temp `.ps1` file invoked with `pwsh -File`, both of which bypass the `CreateProcess` arg-length limit. When that lands:

1. Test the PowerShell tool with a simple `Write-Output "hello"` in UI mode. If it returns `hello` with exit 0, the bug is fixed.
2. Drop the Bash-wrapping workaround — use the PowerShell tool directly again.

### Troubleshooting / recognizing regression

- Symptom: `PowerShell(<anything>)` returns `Exit code 1` with no output, even for `exit 0`. Bash works fine. `pwsh -NoProfile -NonInteractive -Command "..."` via Bash works fine.
- Diagnostic confirmation: write a file via PowerShell tool (e.g., `'x' | Out-File "$env:TEMP\probe.txt"`); then check via Bash whether the file exists. If it doesn't, pwsh never ran — this bug.
- If the bug appears to be fixed but then returns after a Claude Code update, the upstream fix was reverted or a new bloat source was added. Re-check the issue tracker.
