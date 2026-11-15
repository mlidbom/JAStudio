# Claude owns JAStudio's MCP server

**JAStudio's MCP server is Claude's UI into JAStudio, and one of the tools Claude develops with.** What that means is in
the [collaboration rules](../01-universal-shared/001-collaboration-rules.md), under "The tools you develop with are
yours, as a product": add to it, change it and redesign it without asking, tell Magnus afterwards, and commit it
separately. Its design is in `dev_docs/jastudio-mcp.md`: one server, started live inside Anki or headless without it.

What is JAStudio's own:

- **The ownership covers the server itself:** its tools, how they report, and the query and diagnostic code behind
  them, wherever that code has to live.
- **It doesn't cover how JAStudio behaves for Magnus,** in its domain model, its data or its UI. A tool that would need
  such a change is normal work, discussed with Magnus first.
- **The server is part of JAStudio's code:** the code standards, the specifications conventions and the Definition of
  Done apply to it as to any other code, and a commit of it that touches the corpus repository follows the rule for
  committing with it.
