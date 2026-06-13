# Build and test

Always use `venv\Scripts\python.exe` (not bare `python`) — the venv is required for Anki/Qt/pythonnet.

| Task | Command |
|---|---|
| Fast .NET build (iteration) | `dotnet build src\src_dotnet\JAStudio.slnx -c Debug` |
| .NET tests | `dotnet test src\src_dotnet\JAStudio.slnx --verbosity quiet` |
| Python tests | `venv\Scripts\python.exe -m pytest` |
| Lint Python (autofix) | `ruff check --fix` |
| Format Python | `ruff format` |
| Full validation (Definition of Done) | `.\full-build.ps1` |

Run the fast .NET build for iteration. Run `full-build.ps1` only when validating completion or when the .NET
API surface changed — it does .NET build → regenerate Python type stubs → run basedpyright, so stubs need
regenerating whenever the API surface changes.

## Definition of Done

No task is complete until:

- `.\full-build.ps1` succeeds end-to-end: .NET build clean, stubs regenerated, **0 basedpyright errors**.
- All .NET tests pass (`dotnet test src\src_dotnet\JAStudio.slnx`).
- All Python tests pass (`pytest`).

## Python type checking

Use `.\basedpyright-wrapper.bat`, **not** bare `basedpyright` — the wrapper exists because direct
basedpyright invocation breaks IDE source navigation. Pyright runs in the IDE; basedpyright from CLI/CI.
Strict mode is on (`pyproject.toml [tool.pyright]`); don't relax it for individual files without a real
reason.
