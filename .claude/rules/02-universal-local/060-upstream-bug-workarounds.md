# Upstream bug workarounds

Active workarounds live in [CLAUDE.workarounds.md](../../../CLAUDE.workarounds.md). Read it if **the
PowerShell tool returns `Exit code 1` with no output on every call** (use Bash with
`pwsh -NoProfile -NonInteractive -Command "..."` instead), if C# `csharp-ls` probes start returning "No
symbols found" or symbols from the wrong `.slnx`, if `.claude/settings.json` fails to parse, or if you're
setting this repo up on a fresh machine. Currently covers: csharp-ls + Claude Code
([#16360](https://github.com/anthropics/claude-code/issues/16360)) and the PowerShell tool failure in the
VS Code extension UI mode ([#55671](https://github.com/anthropics/claude-code/issues/55671)).
