# Tell Magnus prominently when JAStudio needs a restart

Whenever Magnus needs to restart JAStudio inside Anki to take in a new build, end the response with this heading, then
one line saying what to do:

### JA STUDIO RESTART REQUIRED ###

The line is, for example, "Restart Anki: it takes in the new build when it starts", or, when the corpus's format changed,
"Close Anki and tell me: I'll fast-forward your corpus checkout, then you start it again."

Such as when corpus commits come with code the live JAStudio doesn't run yet (`045-committing-with-the-corpus-repository.md`,
the process for every change to the corpus's notes), or when a change is built that Magnus should use.

**Why (Magnus, 2026-11-10):** a sentence in the text gets missed. And JAStudio running old code over a corpus rebuilt by
new code writes the old code's generated data back into every note Magnus edits.
