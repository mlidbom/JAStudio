# Environment setup

**The Python venv directory must be named exactly `venv/` (never `.venv/`).** All scripts, CI workflows, and
tool configurations expect this path. If you find a `.venv/`, it was created by a misconfigured prior setup —
remove it and use `venv/`.

- **Foreground / interactive agents** (running locally with the human): the environment is already set up.
  Proceed directly to builds and tests.
- **Background agents** (task/explore agents in a fresh worktree): run `.\setup-agent.ps1` once before any
  builds or tests. It's idempotent — enables long paths, initializes submodules, creates the venv, installs
  deps, builds .NET.
- **Linux / CI**: run `./setup-dev.sh` instead. When running .NET tests on Linux, set
  `JASTUDIO_VENV_PATH="$(pwd)/venv"` so pythonnet can find the venv.
