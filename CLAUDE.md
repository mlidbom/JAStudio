# CLAUDE.md

Project instructions for Claude Code. Applies to the main checkout and all git worktrees under `JAStudio.worktrees/`.

## Universal rule

**Don't code until instructed.** The standard workflow is questions and discussion back and forth coming up with what to do — then the user gives the go-ahead to code. Questions are not instructions to start coding, they are questions to be answered.

## Project shape

Hybrid Python + .NET addon for Anki (Japanese-language study).

- **C# / .NET 10** is the primary language. UI (Avalonia), business logic (Core), Python interop, and Anki integration all live in `src/src_dotnet/`. Solution file: `JAStudio.slnx`.
- **Python 3.13** is a **thin integration layer** with Anki only. The project is actively porting UI/business logic from Python to C#. Don't expand Python functionality — prefer moving logic to C#. Python source: `src/jastudio_src/`, `src/jaslib_src/`, `src/jaspythonutils_src/`. Tests: `src/tests/`.
- **Bridge**: `pythonnet` calls .NET from Python. Generated type stubs live in `typings/` so Python sees .NET types.
- **Submodules**:
  - `submodules/Compze` — actively-developed library JAStudio depends on heavily. Wired in as **project references** (not NuGet packages) so JAStudio always builds against the submodule HEAD. Its csprojs are listed under the `Compze/` folder in `JAStudio.slnx`. Compze knows nothing about JAStudio; treat changes to Compze as Compze's own concern (separate repo, separate workflow). Targets net9.0, JAStudio targets net10.0 — that's fine, MSBuild bridges cleanly.
  - `submodules/pythonnet-stub-generator`, `src/jas_database` — excluded from lint/typecheck/search; treat as read-only third-party code.

## Scoped guidance — read these when editing matching files

When editing or reviewing code, read the relevant guideline file before proceeding. The `applyTo:` YAML at the top of each file is Copilot syntax — ignore it but read the rest.

| When working with… | Read |
|---|---|
| Any `**/*.cs` file | `.github/instructions/shared-instructions/csharp-code.instructions.md` **and** `.github/instructions/csharp-code.instructions.md` |
| `**/*.cs` files in `**/*.Specifications/` or `**/Tests/` projects | also `.github/instructions/shared-instructions/csharp-specifications.instructions.md` |
| `**/*.axaml` or `**/*.axaml.cs` files | also `.github/instructions/shared-instructions/axaml-views.instructions.md` |
| Any `**/*.py` file | `.github/instructions/python-code.instructions.md` |
| Authoring or revising `dev_docs/**/*.md` | `.github/instructions/shared-instructions/dev-docs.instructions.md` |

Cross-cutting rules from those files that are easy to forget:

- **Never suppress type errors** (`# pyright: ignore`, `# type: ignore`, or C# equivalents) without explicit permission. Fix the code instead.
- **Never swallow exceptions** in a `catch` — only catch if you have a real recovery strategy or are adding context before re-throwing.
- Every Python file must start with `from __future__ import annotations` (enforced by ruff `isort.required-imports`).

## Commands

Always use `venv\Scripts\python.exe` (not bare `python`) — the venv is required for Anki/Qt/pythonnet.

| Task | Command |
|---|---|
| Fast .NET build (iteration) | `dotnet build src\src_dotnet\JAStudio.slnx -c Debug` |
| .NET tests | `dotnet test src\src_dotnet\JAStudio.slnx --verbosity quiet` |
| Python tests | `venv\Scripts\python.exe -m pytest` |
| Lint Python (autofix) | `ruff check --fix` |
| Format Python | `ruff format` |
| Full validation (Definition of Done) | `.\full-build.ps1` |

### Definition of Done

No task is complete until:

- `.\full-build.ps1` succeeds end-to-end. That means: .NET build clean, stubs regenerated, **0 basedpyright errors**.
- All .NET tests pass (`dotnet test src\src_dotnet\JAStudio.slnx`).
- All Python tests pass (`pytest`).

`full-build.ps1` does .NET build → regenerate Python type stubs → run basedpyright. Run the fast build for iteration; the full build only when validating completion or when the .NET API surface has changed (stubs need regenerating).

## Type checking

Use `.\basedpyright-wrapper.bat`, **not** bare `basedpyright`. The wrapper exists because direct basedpyright invocation breaks IDE source navigation. Pyright is used in the IDE; basedpyright is used from CLI/CI.

Strict mode is on (see `pyproject.toml [tool.pyright]`). Don't relax it for individual files unless you have a real reason.

## Environment setup

**The Python venv directory must be named exactly `venv/` (never `.venv/`).** All scripts, CI workflows, and tool configurations expect this path. If you find a `.venv/`, it was created by a misconfigured prior setup — remove it and use `venv/`.

- **Foreground / interactive agents** (running locally with the user): the human has set up the environment already. Proceed directly to builds and tests.
- **Background agents** (task/explore/agents running in a fresh worktree): run `.\setup-agent.ps1` once before any builds or tests. It's idempotent — enables long paths, initializes submodules, creates the venv, installs deps, builds .NET.
- **Linux / CI**: run `./setup-dev.sh` instead. When running `.NET` tests on Linux, set `JASTUDIO_VENV_PATH="$(pwd)/venv"` so pythonnet can find the venv.

## Workarounds for upstream bugs

Active workarounds live in [CLAUDE.workarounds.md](CLAUDE.workarounds.md). Read it if C# LSP probes start returning "No symbols found", if `.claude/settings.json` fails to parse, or if you're setting this repo up on a fresh machine. Currently covers: csharp-ls + Claude Code [#16360](https://github.com/anthropics/claude-code/issues/16360).

## Serena MCP

When using Serena's semantic tools (`mcp__plugin_serena_serena__*`), the project must be activated first or every call errors with "No active project."

**Always call this once before the first Serena tool use in a session:**

```
mcp__plugin_serena_serena__activate_project(project=<current worktree absolute path>)
```

Do this proactively — don't ask the user. Each worktree (`worktree_1`, `worktree_2`, ...) is its own Serena project; activating one doesn't activate the others. If Serena tools aren't going to be used in a session, no need to activate.

Compze is project-referenced (not nuget-referenced), so both LSP and Serena can navigate into it — `goToDefinition` from a JAStudio call-site lands in `submodules/Compze/src/...` source, and `findReferences` spans both sides. Use whichever is more convenient for the task.

## What not to touch

- `typings/` — auto-generated by `regenerate-stubs.ps1`. Never hand-edit.
- `src/runtime_binaries/` — copied .NET output, regenerated by `copy_libraries.ps1`.
- `src/jastudio_src/_lib/`, `_lib_patched/`, `manually_copied_in_libraries/` — bundled third-party Python libs, excluded from lint/typecheck.
- `submodules/` — separate repos; coordinate changes through their own workflows.
- `.github/workflows/` — CI configuration; don't modify without explicit permission.
- Build artifacts: `bin/`, `obj/`, `venv/`, `.vs/`, `CopilotIndices/`.

## Worktrees

This repo is typically used via git worktrees under `C:\Dev\JAStudio.worktrees\worktree_N`. Don't assume the current directory is the main checkout. When generating paths, prefer paths relative to the worktree root.
