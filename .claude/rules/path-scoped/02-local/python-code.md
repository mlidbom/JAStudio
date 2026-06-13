---
paths:
  - "**/*.py"
---

# Python code conventions for JAStudio

Python is a thin Anki-integration layer being ported to C# — **don't expand Python functionality; prefer
moving logic to C#** (see [020-project-orientation.md](../../02-universal-local/020-project-orientation.md)).

## Type safety
- **Every function has complete type hints** (parameters and return). Use `typing` / `collections.abc` for
  generics.
- Satisfy basedpyright by fixing the code — never suppress (see
  [080-never-suppress-diagnostics.md](../../02-universal-local/080-never-suppress-diagnostics.md)).
- **Every file must start with `from __future__ import annotations`** — enforced by ruff
  (`isort.required-imports` in `pyproject.toml`). Missing it fails lint and can hide other type warnings.

## Calling C#
- Import the auto-generated stubs in `typings/` for C# APIs (e.g.
  `from JAStudio.Core.Note import KanjiNote`). C# exceptions propagate as Python exceptions — handle them
  appropriately.
- **Stubs can be stale.** If a C# API just changed, run `.\full-build.ps1` (it regenerates stubs) before
  relying on `typings/` for the new surface.

## Testing
- Test dirs: `src/tests/{jastudio_tests,jaslib_tests,jaspythonutils_tests}/` — choose by what's under test.
  Follow existing pytest patterns; type-hint test code too. Run: `pytest`.

## Style
- PEP 8, 4-space indentation, line length effectively unlimited (`line-length = 320`). f-strings;
  comprehensions over `map`/`filter` where readable. **ruff** is the sole linter (`ruff check --fix`).
- Don't add comments unless they match existing style or explain genuinely complex logic; prefer
  self-documenting code; update comments when changing code.
