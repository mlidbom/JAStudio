"""Copies .NET build output to runtime_binaries/ before loading the CLR.

This runs at Anki addon startup, *before* pythonnet loads any assemblies,
so the DLLs are not locked and can always be overwritten.
"""

from __future__ import annotations

import hashlib
import shutil
from typing import TYPE_CHECKING

from jastudio import mylog

if TYPE_CHECKING:
    from pathlib import Path

_FILE_PATTERNS = ("*.dll", "*.deps.json", "*.runtimeconfig.json", "*.staticwebassets.runtime.json", "*.staticwebassets.endpoints.json")
_MECAB_DICTIONARY_FOLDER_NAME = "dic"


def _find_build_output(src_dotnet: Path) -> list[Path]:
    """Find the most recently built output directories for JAStudio.UI and JAStudio.Web."""
    results: list[Path] = []
    for project_name, marker_dll in [("JAStudio.UI", "JAStudio.UI.dll"), ("JAStudio.Web", "JAStudio.Web.dll"), ("JAStudio.Anki.PythonInterop", "JAStudio.Anki.PythonInterop.dll")]:
        bin_root = src_dotnet / project_name / "bin"
        if not bin_root.exists():
            continue

        candidates: list[tuple[float, Path]] = [(marker.stat().st_mtime, marker.parent) for marker in bin_root.rglob(marker_dll)]

        if candidates:
            candidates.sort(key=lambda c: c[0], reverse=True)
            results.append(candidates[0][1])

    return results


def _md5(path: Path) -> str:
    h = hashlib.md5()
    with open(path, "rb") as f:
        for chunk in iter(lambda: f.read(8192), b""):
            h.update(chunk)
    return h.hexdigest()


def _copy_unless_identical(src_file: Path, dest_file: Path) -> bool:
    """Copy src_file over dest_file unless dest_file already has identical content. Returns whether a copy was made."""
    if dest_file.exists() and _md5(src_file) == _md5(dest_file):
        return False
    shutil.copy2(src_file, dest_file)
    return True


def copy_binaries(workspace_root: Path) -> None:
    """Copy .NET assemblies from the build output to runtime_binaries/.

    Skips files whose MD5 already matches (no-op when nothing changed).
    Keeps the existing runtime_binaries if no build output exists yet.
    """
    src_dotnet = workspace_root / "src_dotnet"
    dest = workspace_root / "runtime_binaries"

    build_outputs = _find_build_output(src_dotnet)
    if not build_outputs:
        mylog.info("No .NET build output found — using existing runtime_binaries (if any)")
        return

    dest.mkdir(parents=True, exist_ok=True)

    copied = 0
    for build_output in build_outputs:
        for pattern in _FILE_PATTERNS:
            for src_file in build_output.glob(pattern):
                if _copy_unless_identical(src_file, dest / src_file.name):
                    copied += 1

        mecab_dictionary = build_output / _MECAB_DICTIONARY_FOLDER_NAME
        if mecab_dictionary.is_dir():
            dest_mecab_dictionary = dest / _MECAB_DICTIONARY_FOLDER_NAME
            dest_mecab_dictionary.mkdir(parents=True, exist_ok=True)
            for src_file in mecab_dictionary.iterdir():
                if src_file.is_file() and _copy_unless_identical(src_file, dest_mecab_dictionary / src_file.name):
                    copied += 1

    if copied > 0:
        mylog.info(f"Runtime binaries: {copied} copied")
    else:
        mylog.info("Runtime binaries: all files up to date")
