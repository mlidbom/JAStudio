# Claude owns JAStudio's MCP server — improve it proactively, without asking

**JAStudio's MCP server is Claude's UI into JAStudio, and Claude owns it as a product.** Its design is in
`dev_docs/jastudio-mcp.md`: one server, started live inside Anki or headless without it. It's this project's case of the
collaboration rule "Build the tools the work will need again now, not later — and propose the big ones".

- **Be proactive.** Whenever a task would go faster or better with a tool the server doesn't have, or with an existing
  tool changed, add or change it then, as part of the work. Don't work around a missing tool with throwaway programs,
  repeated manual steps or guesses: that's the signal to improve the server. Removing or redesigning tools that turn out
  unhelpful is just as much part of owning it.
- **No permission needed.** Don't ask before adding to or changing the server. Tell Magnus afterwards, in a line or two,
  what you added or changed and why you needed it.
- **Commit it separately, whenever it suits the work.** Changes to the server go in commits of their own, apart from the
  work that needed them, so each kind of change is easy to follow. They need no go-ahead from Magnus to be committed
  (Magnus, 2026-11-09). The usual checks still come first: the Definition of Done passes, and a commit
  that touches the corpus repository follows the rule for committing with it.

**What the ownership covers:** the server itself, meaning its tools, how they report, and the query and diagnostic code
behind them, wherever that code has to live. **What it doesn't cover:** changing how JAStudio behaves for Magnus, in its
domain model, its data or its UI. A tool that would need such a change is normal work, discussed with Magnus first.

The server is part of JAStudio's code, so the code standards, the specifications conventions and the Definition of Done
apply to it as to any other code.
