# Rules folder

Replaces CLAUDE.md — project rules, read by both humans and the agent.

- `01-universal-shared` — always-on shared standards (a directory symlink into the `.claude-shared/` catalog)
- `02-universal-local/` — always-on JAStudio rules, such as orientation, repository structure, build/test and
  committing with the corpus repository
- `path-scoped/` — rules that load only when touching matching files; `01-shared/` links + `02-local/` files

**Start here:** [`path-scoped/01-shared/rules-folder.md`](path-scoped/01-shared/rules-folder.md) — how the
folder is loaded, the include order, and the `name.rationale.txt` rationale-sidecar convention.

**Shared vs local is visible in the names:** everything pulled from the `.claude-shared/` catalog (a git
worktree; see `.claude-shared/README.md`) sits behind a `…-shared` entry — `01-universal-shared` is a single
directory symlink to the catalog's universal rules; `path-scoped/01-shared/` holds per-file symlinks to the
adopted path-scoped rules. Project-specific rules live alongside as regular files (`02-universal-local/`,
`path-scoped/02-local/`). Skills link flat in `.claude/skills/` with a `shared-` prefix on the link name —
grouping folders break skill discovery and the link folder's name becomes the skill's name, so the prefix
marks shared skills both in the tree and at invocation; see the catalog README. JAStudio's own skills are regular
folders beside the links, without the prefix, such as `changing-what-the-corpus-stores`. If the links look wrong, run
`.claude-shared/git-scripts/verify-claude-symlinks.ps1`. If `.claude-shared/` is
missing, the next session start makes it, or run `sh .claude/verify-shared-claude-config.sessionstart-hook.sh`.
To sync with upstream, run `sh .claude-shared/git-scripts/pull.sh` or `push.sh`.

(This README is `.txt` on purpose — a `README.md` with no frontmatter would inject into every session. Like
every `.txt` doc here, it's still written in Markdown.)
