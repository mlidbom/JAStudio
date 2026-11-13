# C# code-intelligence in JAStudio

The generic guidance — the ReSharper-backed `dotnet-analysis-and-refactoring` MCP first (addressed by symbol name), the
built-in `jetbrains-rider` MCP for non-semantic work, `sherlock` for referenced-library APIs — lives in a
user-level rule on the dev machine; those MCPs require Rider running locally. JAStudio specifics:

- **Compze is a NuGet dependency, not project-referenced.** Going to a definition in Compze lands in decompiled
  metadata, and finding usages covers only the JAStudio side. For Compze's API surface use the `sherlock`
  MCP — it reflects over the `Compze.*` DLLs copied into each project's `bin`; grep the package `.xml` for
  docs.
- **When using the `dotnet-analysis-and-refactoring` MCP by symbol name, pass as `solutionName` the full path of the
  `.slnx` of the checkout you work in,** such as `C:\Dev\JAStudio.worktrees\worktree_1\JAStudio.slnx`. Several
  solutions are usually open, among them the main checkout and its worktrees, which all have the name `JAStudio`. The
  name `JAStudio` picks the main checkout, whichever checkout you're in: a worktree's file is then "not found in
  solution", and a symbol searched by name is found silently in the main checkout's code (verified 2026-11-13).
  For the `jetbrains-rider` MCP pass `rootFolder` = the `.slnx`'s parent folder, which is the checkout's root.
