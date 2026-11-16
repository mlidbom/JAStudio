# Environment setup

**The Python venv directory must be named exactly `venv/` (never `.venv/`).** All scripts, CI workflows, and
tool configurations expect this path. If you find a `.venv/`, it was created by a misconfigured prior setup —
remove it and use `venv/`.

**Linux / CI:** run `./setup-dev.sh` to set the environment up. When running .NET tests on Linux, set
`JASTUDIO_VENV_PATH="$(pwd)/venv"` so pythonnet can find the venv.
