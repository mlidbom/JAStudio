# Universal rules
- **Don't code until instructed to**. Standard workflow is questions and discussion back and forth coming up with what to do. Then I give the go ahead to code. Questions are not instructions to start coding, they are questions to be answered.

# JAStudio Workspace Instructions for AI Assistants

JAStudio is a Japanese language learning tool that runs as an Anki addon. The core logic and UI are in C#/Avalonia, loaded into Anki's Python process via pythonnet. Python serves only as a thin integration layer with Anki's APIs.

## Background Agent Setup

**⚠️ CRITICAL for background agents (task/explore/etc):**

If you are a **background agent** running in a VS Code worktree, you MUST run this setup script before attempting any builds or tests:

```powershell
.\setup-agent.ps1
```

This script will:
- Enable Git long paths (required for Compze submodule)
- Initialize git submodules
- Create Python venv and install dependencies
- Build the .NET solution

The script is idempotent and will skip steps that are already complete.

**⚠️ For foreground/local agents:** Do NOT run `.\setup-agent.ps1` - the human developer has already set up their environment. You can proceed directly to builds and tests.

## Quick Reference

```powershell
dotnet build JAStudio.slnx -c Debug   # Fast .NET build (iteration)
dotnet test JAStudio.slnx -v quiet    # .NET specifications (tests)
pytest                                                 # Python tests
.\full-build.ps1                                       # Full validation (Definition of Done)
ruff check --fix                                       # Lint + autofix Python
ruff format                                            # Format Python
```

## Tech Stack

**Languages & Frameworks:**
- **C# / .NET 10** - Primary language for UI (Avalonia) and business logic
- **Python 3.13** - Thin integration layer with Anki, minimal usage preferred
- **Avalonia UI** - Cross-platform .NET UI framework
- **PyQt6** - Required for Anki addon integration (Anki's UI is Qt-based)

**Key Tools:**
- **basedpyright** - Python type checker (strict, zero errors required)
- **ruff** - Python linter and formatter (configured in `pyproject.toml`)
- **pytest** - Python testing framework
- **dotnet test** - .NET testing framework
- **pythonnet** - Python ↔ C# interop bridge

**Dependencies:**
- Janome, jamdict, pykakasi, python-romkan-ng (Japanese language processing)
- beartype, typing_extensions, deepdiff, pyperclip, autoslot (Python utilities)

## Boundaries & Constraints

**DO NOT:**
- Edit auto-generated files: `runtime_binaries/`, `typings/`
- Modify `.github/workflows/` directory (CI configuration)
- Suppress type errors with `# pyright: ignore` or `# type: ignore` without explicit permission
- Swallow exceptions without re-throwing (see Exception Handling section)
- Add significant Python logic (prefer C# unless interfacing with Anki)
- Touch build artifacts: `bin/`, `obj/`, `venv/`, `.vs/`, `CopilotIndices/`

**DO:**
- Put UI logic in C# (`JAStudio.UI`)
- Put business logic in C# (`JAStudio.Core`)
- Use Python only as thin Anki integration layer
- Re-throw or wrap exceptions with context
- Fix type errors properly rather than suppressing them

## How to Build

```powershell
dotnet build JAStudio.slnx -c Debug # Fast build: compiles .NET only

.\full-build.ps1                                    # Full build: compiles .NET + regenerates Python type stubs + runs basedpyright
```

**Use the full build** when you are done making changes and need to validate everything (Definition of Done).
The fast build is for quick iteration — the Anki addon copies .NET binaries on its own startup, and stubs only need regenerating when the .NET API surface changes.

### How to Test

**On Windows (PowerShell):**
```powershell
dotnet test JAStudio.slnx --verbosity quiet
pytest
```

**On Linux/CI** (set `JASTUDIO_VENV_PATH` so pythonnet can find the venv):
```bash
JASTUDIO_VENV_PATH="$(pwd)/venv" dotnet test JAStudio.slnx --verbosity quiet --filter "FullyQualifiedName!~BulkLoaderTests"
source venv/bin/activate && pytest
```


**Test Guidelines:**
- Write tests for new C# features in the appropriate `*.Specifications` project
- Python test directories: `src/tests/jaspythonutils_tests/`, `src/tests/jaslib_tests/`, `src/tests/jastudio_tests/` — choose based on what code is under test
- Follow existing test patterns and naming conventions
- Run relevant tests after making changes, not just at the end

### Definition of Done

No task should be considered complete until:
- `.\full-build.ps1` succeeds (compiles .NET, regenerates stubs, **0 basedpyright errors**)
- All .NET tests pass
- All Python tests pass

### Python Type Errors

Do **not** suppress type errors with `# pyright: ignore` or `# type: ignore` comments. The code should almost always be fixed or restructured to satisfy the type checker. If you cannot find a way to make the type checker understand the code, ask for permission before adding a suppression.

## Background Agent Setup (Linux/CI)

When running on Linux (e.g. GitHub Actions runners):

```bash
./setup-dev.sh                      # One-time: creates venv, installs deps, builds .NET
```

Then use the test commands from the "How to Test" section above.

## Project Architecture

### Python as Thin Layer
This project is actively porting UI from Python/PyQt6 to C#/Avalonia. **Use Python as little as possible:**

- ✅ Menu logic, definitions, and handlers belong in C# (`JAStudio.UI`)
- ✅ Business logic belongs in C# (`JAStudio.Core`)
- ❌ Python should only be a thin integration layer with Anki

### Directory Structure
- `dev_docs/` - Development progress notes and porting status documents
- `src/src_dotnet/` - C# source code (Avalonia UI, Core logic, Python interop)
  - `JAStudio.UI/` - Avalonia UI
  - `JAStudio.UI.DesktopHost/` - Desktop entry point for running Avalonia UI outside Anki
  - `JAStudio.Core/` - Domain logic
  - `JAStudio.Anki/` - Anki integration utilities (C# side)
  - `JAStudio.PythonInterop/` - Python ↔ C# bridge
  - `JAStudio.Core.Specifications/`, `JAStudio.UI.Specifications/` - .NET test projects (xunit)
- `src/jastudio_src/` - Python source (Anki addon, thin integration layer)
- `src/jaspythonutils_src/` - Python utility libraries
- `src/jaslib_src/` - Python libraries (Japanese language processing)
- `src/tests/` - Python tests (`jaspythonutils_tests/`, `jaslib_tests/`, `jastudio_tests/`)
- `src/jas_database/` - Snapshot data for Japanese language database
- `src/web/` - HTML templates and styles for Anki card rendering
- `src/user_files/` - User-specific configuration files
- `src/runtime_binaries/` - Compiled .NET DLLs (auto-generated, don't edit)
- `typings/` - Python type stubs for C# (auto-generated, don't edit)

## Python Environment

The project uses a virtual environment at `venv/`. Python dependencies are managed via `requirements.txt`.
On Linux, run `./setup-dev.sh` to create the venv and install all dependencies.

**⚠️ CRITICAL:** The virtual environment directory MUST be named `venv/` (not `.venv/`). All scripts, CI workflows, and tool configurations expect this exact path. If you encounter a `.venv/` directory, it was created by a misconfigured prior setup and should be removed. Always create or use `venv/` instead.