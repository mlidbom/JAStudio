# C# code-intelligence in JAStudio

The generic guidance — the ReSharper-backed `dotnet-analysis-and-refactoring` MCP first (addressed by symbol name), the
built-in `jetbrains-rider` MCP for non-semantic work, `sherlock` for referenced-library APIs — lives in a
user-level rule on the dev machine; those MCPs require Rider running locally. JAStudio specifics:

- **Compze is a NuGet dependency, not project-referenced.** Going to a definition in Compze lands in decompiled
  metadata, and finding usages covers only the JAStudio side. For Compze's API surface use the `sherlock`
  MCP — it reflects over the `Compze.*` DLLs copied into each project's `bin`; grep the package `.xml` for
  docs.
- **When using the Rider/ReSharper MCPs, pass `solutionName: 'JAStudio'`** — multiple solutions are usually
  open, so omitting it errors or answers from the wrong one. For the `jetbrains-rider` MCP also pass
  `rootFolder` = the `.slnx`'s parent folder, which is the worktree root (`<worktree>`).
