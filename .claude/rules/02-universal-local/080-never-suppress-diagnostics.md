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
