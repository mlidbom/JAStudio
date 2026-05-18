# CLAUDE.md — `src/src_dotnet/`

C# code conventions for this project. The root [CLAUDE.md](../../CLAUDE.md) has overall project context, commands, and the Definition of Done — read that first if you haven't.

## Always read before editing `.cs` files in this tree

Two files — both apply to every `.cs` file:

1. **`.github/instructions/shared-instructions/csharp-code.instructions.md`** — cross-repo C# conventions (formatting, naming, null handling, no records, default-interface-method mixins, async `.caf()`, exception handling, etc.). The `applyTo:` YAML at the top is Copilot syntax — ignore it but read the rest.
2. **`.github/instructions/csharp-code.instructions.md`** — JAStudio-specific architecture rules (Avalonia + MVVM, CommunityToolkit.Mvvm `[ObservableProperty]` with `_camelCase` fields, JAStudio.Core as pure domain, PythonInterop as the only Python bridge, NuGet packages OK).

## Additional guidance by file pattern

| If editing… | Also read |
|---|---|
| `.cs` files in any `*.Specifications/` project | `.github/instructions/shared-instructions/csharp-specifications.instructions.md` — BDD nested specifications with `[XF]`, the `Must` assertion library, container-resolved black-box specs (not unit tests), naming rules where the full spec path must read as a sentence. |
| `.axaml` or `.axaml.cs` files | `.github/instructions/shared-instructions/axaml-views.instructions.md` — compiled bindings (`x:DataType`), DataContext set in code-behind only, commands over event handlers, parameterless XAML-designer constructors. |

## Project layout in this directory

- `JAStudio.UI/` — Avalonia UI (Views in `.axaml`, ViewModels in `.cs`)
- `JAStudio.UI.DesktopHost/` — desktop entry point for running the UI outside Anki
- `JAStudio.Core/` — pure domain logic, no UI / Anki / Python dependencies
- `JAStudio.Core.Specifications/`, `JAStudio.UI.Specifications/`, `JAStudio.Web.Specifications/`, `JAStudio.Anki.PythonInterop/`, etc. — `*.Specifications/` projects use xUnit v3
- `JAStudio.PythonInterop/` — the only layer allowed to bridge C# ↔ Python (pythonnet)
- `JAStudio.Anki/` — C#-side Anki integration utilities (no direct pythonnet — that belongs in PythonInterop)
- `JAStudio.Web/` — in-process Kestrel/Blazor card-rendering server (see `dev_docs/web-rendering-architecture.md`)
- `JAStudio.Dictionary/` — Japanese dictionary integration
