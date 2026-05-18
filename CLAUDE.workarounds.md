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
