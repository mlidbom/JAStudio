# Project orientation

Hybrid **Python + .NET** addon for **Anki** (Japanese-language study).

- **C# / .NET 10 is the primary language.** UI (Avalonia), business logic (Core), Python interop, and Anki
  integration all live in `src/src_dotnet/`. Solution: `JAStudio.slnx` at the repo root.
- **Python 3.13 is a thin integration layer with Anki only.** The project is actively porting UI/business
  logic from Python to C# — **don't expand Python functionality; prefer moving logic to C#.** Python source:
  `src/jastudio_src/`, `src/jaslib_src/`, `src/jaspythonutils_src/`. Tests: `src/tests/`.
- **Bridge:** `pythonnet` calls .NET from Python. Generated type stubs live in `typings/` so Python sees
  .NET types.
- **Compze** — a library JAStudio depends on heavily, consumed as **published NuGet packages** (from
  nuget.org; versions pinned per-package in the consuming `.csproj`s — see the `Compze.*` `PackageReference`s).
  Compze knows nothing about JAStudio; treat changes to Compze as Compze's own concern (separate repo,
  separate workflow). Its API is pre-1.0 and may change between versions, so bumping Compze is a deliberate
  version raise, not automatic. For its API surface use the `sherlock` MCP, not source navigation — see
  [070-csharp-code-intelligence-in-jastudio.md](070-csharp-code-intelligence-in-jastudio.md).

## Worktrees

This repo is typically used via git worktrees under `C:\Dev\JAStudio.worktrees\worktree_N` (these rules apply
to the main checkout and all such worktrees). Don't assume the current directory is the main checkout —
prefer paths relative to the worktree root.
