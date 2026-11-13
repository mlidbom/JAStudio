# C# code-intelligence in JAStudio

The generic guidance — the ReSharper-backed `resharper-joshua` MCP first (addressed by symbol name), the
built-in `jetbrains-rider` MCP for non-semantic work, `sherlock` for referenced-library APIs — lives in a
user-level rule on the dev machine; those MCPs require Rider running locally. JAStudio specifics:

- **Compze is a NuGet dependency, not project-referenced.** `goToDefinition` into Compze lands in decompiled
  metadata, and `findReferences` covers only the JAStudio side. For Compze's API surface use the `sherlock`
  MCP — it reflects over the `Compze.*` DLLs copied into each project's `bin`; grep the package `.xml` for
  docs.
- **When using the ReSharper MCP, pass as `solutionName` the full path of the `.slnx` of the checkout you work
  in,** such as `C:\Dev\JAStudio.worktrees\worktree_1\JAStudio.slnx`. Several solutions are usually open, among
  them the main checkout and its worktrees, which all have the name `JAStudio`. The name `JAStudio` picks the
  main checkout, whichever checkout you're in: a worktree's file is then "not found in solution", and a symbol
  searched by name is found silently in the main checkout's code (verified 2026-11-13).
  For the `jetbrains-rider` MCP pass `rootFolder` = the `.slnx`'s parent folder, which is the checkout's root.
- **`csharp-ls` doesn't watch the solution graph.** After editing a `.csproj` or `.slnx` (or a `git checkout`
  / submodule update that moves either), restart Claude Code so it re-indexes; `.cs` changes (new files,
  deletes, renames) are picked up live. csharp-ls solution pinning is itself a workaround — see
  [060-upstream-bug-workarounds.md](060-upstream-bug-workarounds.md).
