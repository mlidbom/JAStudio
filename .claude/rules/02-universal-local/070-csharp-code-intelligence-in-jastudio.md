# C# code-intelligence in JAStudio

The generic guidance — the ReSharper-backed `resharper-joshua` MCP first (addressed by symbol name), the
built-in `jetbrains-rider` MCP for non-semantic work, `sherlock` for referenced-library APIs — lives in a
user-level rule on the dev machine; those MCPs require Rider running locally. JAStudio specifics:

- **Compze is a NuGet dependency, not project-referenced.** `goToDefinition` into Compze lands in decompiled
  metadata, and `findReferences` covers only the JAStudio side. For Compze's API surface use the `sherlock`
  MCP — it reflects over the `Compze.*` DLLs copied into each project's `bin`; grep the package `.xml` for
  docs.
- **When using the Rider/ReSharper MCPs, pass `solutionName: 'JAStudio'`** — multiple solutions are usually
  open, so omitting it errors or answers from the wrong one. For the `jetbrains-rider` MCP also pass
  `rootFolder` = the `.slnx`'s parent folder, which is the worktree root (`<worktree>`).
- **`csharp-ls` doesn't watch the solution graph.** After editing a `.csproj` or `.slnx` (or a `git checkout`
  / submodule update that moves either), restart Claude Code so it re-indexes; `.cs` changes (new files,
  deletes, renames) are picked up live. csharp-ls solution pinning is itself a workaround — see
  [060-upstream-bug-workarounds.md](060-upstream-bug-workarounds.md).
