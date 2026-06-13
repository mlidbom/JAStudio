---
paths:
  - "**/*.cs"
---

# C# code conventions for JAStudio

Design principles (OO, everything-in-its-place, naming, comments) live in the universal rules
(`01-universal-shared`). Formatting and idiom-level style — indentation, braces, `var`, expression bodies,
casing, namespace = folder path — is enforced by the ReSharper/Rider profile and surfaces as inspections with
quick-fixes, so it is not documented here. This file holds only JAStudio's architecture rules.

## Architecture

### UI — `JAStudio.UI`
- Use **Avalonia UI** for all new UI. Follow **MVVM**: Views (`.axaml`) + ViewModels (`.cs`).
- ViewModels must not reference Avalonia types — keep them testable and UI-framework-agnostic.
- Use **CommunityToolkit.Mvvm** for change notification (`[ObservableProperty]`) and commands
  (`RelayCommand`, `AsyncRelayCommand`).
- `[ObservableProperty]` fields are `_camelCase`; the source generator creates the PascalCase property
  without the underscore (`_isInflectingWord` → `IsInflectingWord`).

### Business logic — `JAStudio.Core`
- Pure domain logic: no UI, Anki, or Python dependencies. Prefer dependency injection and immutable types
  (readonly properties, private setters).

### Python interop — `JAStudio.PythonInterop`
- The **only** layer that bridges C# ↔ Python. Expose clean C# APIs that hide pythonnet complexity; convert
  Python exceptions to appropriate C# exceptions; document Python type mappings.

### Anki integration — `JAStudio.Anki`
- C#-side Anki utilities. No direct pythonnet here — that belongs in PythonInterop.

## NuGet packages

JAStudio is an end-user application, not a library. **Freely add NuGet packages** when they're the right tool
for the job — the goal is the best, most maintainable result, not minimal dependencies.
