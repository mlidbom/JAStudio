# Never suppress diagnostics — fix the code

**Never suppress type errors or warnings without explicit permission.** The default response to a type error,
analyzer warning, or compiler warning is to understand it and fix the real problem — not to silence the
messenger.

- Python: no `# pyright: ignore`, no `# type: ignore`.
- C#: no `#pragma warning disable`, no `// ReSharper disable`, no attribute suppressions, no severity
  downgrades, and no null-forgiving `!` used to dodge a real nullability warning.

Suppression is the rare exception, justified only when the diagnostic genuinely doesn't fit the site — never
because the proper fix is work. If you believe a suppression is right, propose it and let Magnus decide, and
put the rationale on the same line as the directive.

**A provable false positive needs no asking:** suppress it, and say so in the report (Magnus, 2026-11-18: "Any
warning that is provably a false positive, such as this one, you do not need to ask permission to suppress. Just do
it"). It's provable when what the diagnostic claims is untrue of the code, and one line can say why, such as
`NotAccessedPositionalProperty.Global` on a record whose value only its equality uses. A warning that's right, but
inconvenient to fix, is still a question for Magnus.

- **As narrow as it fits:** `disable once` on the line, or the file when every instance there is the same case.
- **The reason on the directive's line,** after the id and a space, without commas, which ReSharper reads as
  separating ids: `// ReSharper disable NotAccessedPositionalProperty.Global because the index only compares a key's
  value and never reads it`. Check with an inspection afterwards that it took.

**Approved exception — BDD specification files:** put `// ReSharper disable InconsistentNaming` after the
usings, as the existing specifications do. BDD class and method names are `lower_case_with_underscores` by
design, which ReSharper's naming rule flags.

**Approved exception — files of tevent interfaces** (Magnus, 2026-11-16): the same line after the usings, as in
`IVocabTevent.cs`. Nested tevent interfaces are named without an `I`, by Compze's semantic-events convention, so that
`IVocabTevent.Forms.Added` reads as what happened. And `// ReSharper disable PossibleInterfaceMemberAmbiguity`, which a
part's `PropertyUpdated` level hiding its note's sets off on every tevent: "I'm pretty sure that I routinely just suppress
that warning and have never encountered a real problem" (Magnus, 2026-11-16).
