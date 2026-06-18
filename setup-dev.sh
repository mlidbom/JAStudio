#!/usr/bin/env bash
# Development environment setup for Linux (used by CI and coding agents).
# Creates a Python venv, installs dependencies, and builds the .NET solution.
#
# Usage:
#   ./setup-dev.sh              # full setup
#   ./setup-dev.sh --skip-build # skip the .NET build step

set -euo pipefail
cd "$(dirname "$0")"

SKIP_BUILD=false
for arg in "$@"; do
  case "$arg" in
    --skip-build) SKIP_BUILD=true ;;
  esac
done

# ── Python venv ──────────────────────────────────────────────────────────────
if [ ! -d venv ]; then
  echo "=== Creating Python venv ==="
  python3 -m venv venv
fi

echo "=== Installing Python dependencies ==="
# shellcheck disable=SC1091
source venv/bin/activate

pip install --quiet --upgrade pip

# Install packages needed by .NET tests (pythonnet, Janome, jamdict)
# and by Python tests (pytest, beartype, etc.)
pip install --quiet \
  pytest \
  beartype \
  typing_extensions \
  deepdiff \
  pyperclip \
  autoslot \
  Janome \
  pythonnet \
  "jamdict==0.1a11.post2" \
  "jamdict-data-fix==1.5.1a2"

# ── .NET build ───────────────────────────────────────────────────────────────
if [ "$SKIP_BUILD" = false ]; then
  echo "=== Building .NET solution ==="
  dotnet build JAStudio.slnx -c Debug
fi

echo "=== Setup complete ==="
echo "To run .NET tests:    JASTUDIO_VENV_PATH=\"\$(pwd)/venv\" dotnet test JAStudio.slnx --verbosity quiet"
echo "To run Python tests:  source venv/bin/activate && pytest src/tests"
