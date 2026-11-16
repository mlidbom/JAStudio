# The structure of the notes' tevents

> **Status: step 6 of [the migration](the-migration.md), revised 2026-11-16 after Magnus's look, with `jastudio-rep-opus`.**
> Interfaces and their classes only. Nothing publishes or subscribes to them yet. Branch
> `migrating-the-notes-to-the-state-store`, in `src/src_dotnet/JAStudio.Core/`: `Note/Tevents/`,
> `Note/NoteFields/Tevents/` and `Note/Vocabulary/Tevents/`, a file with every interface of a taggregate's tevents and
> one with their classes.

## The headline

**A note's tevents mirror the note's parts, and keep domain tevents apart from `PropertyUpdated` tevents** (Magnus,
2026-11-16).

- **Every note tevent is an `INoteTevent`,** and each note class has a root of its own below it, such as `IVocabTevent`.
- **Each part of a note that changes has a group below its note's root,** which is itself a tevent: `IVocabTevent.Forms`.
- **A part's `PropertyUpdated` tevents say which of its properties got which value,** carrying the whole new value:
  `IVocabTevent.Forms.PropertyUpdated.EnteredForms`. The part's tevent appliers apply only these, so however many domain
  tevents later set a property, no applier changes.
- **A domain tevent says what happened beyond the value,** and is also the `PropertyUpdated` tevent it causes:
  `IVocabTevent.Forms.Added : PropertyUpdated.EnteredForms`, with the form added and the whole resulting list.
- **A domain tevent exists only where something reacts to more than the new value.** This domain is mostly edits of
  values, so most tevents are `PropertyUpdated` only, and honestly so. The tevents aren't stored, so a domain tevent can be
  added the day something needs it.
- **A part that several notes have,** such as a text field a person edits, is a shared tomponent with a hierarchy of its own,
  `IStringValueTevent`, which each of its places in a note adopts under the place's name: `IVocabTevent.Mnemonic<T>`.

So a subscriber says exactly what it follows:

```csharp
//Any change to any vocab's forms: what the vocab matches in sentences changed.
observe.ForTevent((IVocabTevent.Forms it) => ...);

//A form added: the vocab whose question it is get this vocab's question in turn, in the same unit of work.
handle.ForTevent((IVocabTevent.Forms.Added it, ITaggregateStoreUpdater notes) => ...);

//A vocab renamed: the word-references to its old name, in other notes, are renamed.
handle.ForTevent((IVocabTevent.Question.Renamed it, ITaggregateStoreUpdater notes) => ...);

//A vocab's mnemonic set, and nothing else about it.
observe.ForTevent((IVocabTevent.Mnemonic<IStringValueTevent.PropertyUpdated.Value> it) => ...);
```

## The vocab's tevents so far

```
IVocabTevent : INoteTevent
   Created : ITaggregateCreatedTevent, Question.PropertyUpdated.Text
   Question
      PropertyUpdated.Text            { Text }
      PropertyUpdated.Disambiguation  { Disambiguation }
      Renamed : PropertyUpdated.Text, PropertyUpdated.Disambiguation  { From }
   Forms
      PropertyUpdated.EnteredForms    { EnteredForms }
      Added   : PropertyUpdated.EnteredForms  { Form }
      Removed : PropertyUpdated.EnteredForms  { Form }
   CorpusAnswer<T>, Explanation<T>, ExplanationLong<T>, Mnemonic<T>, TechnicalNotes<T>, References<T>   where T : IStringValueTevent

IStringValueTevent
   PropertyUpdated.Value  { Value }
```

- **The domain tevents, and what reacts to each:**
  - `Created`, a new vocab.
  - `Question.Renamed`: the word-references to the vocab's old name are renamed. It carries the old name because nothing
    else has it: inside a unit of work, the vocab read is its own copy, already renamed. Changing the disambiguation renames
    the vocab too, so `Renamed` sets both properties.
  - `Forms.Added` and `Forms.Removed`: the vocab whose question the form is get or lose this vocab's question in turn.
- **Owning a form, or no longer owning it, is `PropertyUpdated.EnteredForms` only** for now: nothing reacts to more.
- **`PropertyUpdated` is a static class,** a name for the group, as in Compze's sample. As an interface its tevents
  would derive from it and so inherit their own names, and a property named as its tevent then hides that (CS0108). That
  is a consequence of naming nested tevents without an `I`. The part's group, `Forms`, stays a tevent.

## How `VocabNoteForms` will publish and apply them

A sketch, not compiled: how a class declares its tevent appliers comes with Compze's step 3. Its shape is that of
`jastudio-rep-opus`'s experiment, Compze's branch `taggregate-experiments-opus`,
`test/Compze.Tests.Unit/CQRS/Taggregates/PerClassTeventRouting/VocabNote.cs`, and its names are placeholders until the
naming pass.

```csharp
public class VocabNote : JPNote
{
   //The vocab's table extends every note's, and names the vocab's parts: a part is routed to only when its owner's table names it.
   static readonly TeventRouting<VocabNote> Appliers = NoteAppliers.Extend<VocabNote>(it => it
      .Component(static vocab => vocab.Question)
      .Component(static vocab => vocab.Forms)
      .SharedTomponent(static vocab => Slots.Mnemonic));   //and the other five text fields

   //The places of the shared tomponents: statics of the class, as a place describes the class, not one vocab.
   static class Slots
   {
      public static readonly SharedTomponentSlot<...> Mnemonic = new(typeof(VocabTevent.Mnemonic<>));
   }
}

public class VocabNoteForms : TeventiveComponent<VocabNote, INoteTevent, NoteTevent, VocabNoteForms, IVocabTevent.Forms, VocabTevent.Forms>
{
   List<EnteredForm> _enteredForms;

   //The one applier: every tevent about the forms states the whole new list.
   static readonly TeventRouting<VocabNoteForms> Appliers = TeventRouting<VocabNoteForms>.DeclareAppliers(it => it
      .For<IVocabTevent.Forms.PropertyUpdated.EnteredForms>(static (forms, tevent) => forms._enteredForms = [.. tevent.EnteredForms]));

   public void Add(string form)
   {
      if(IsTheQuestion(form) || EnteredFormsSet().Contains(form)) return; //A tevent is published only when something changes.
      Publish(new VocabTevent.Forms.Added(form, [.. _enteredForms, EnteredForm.NotOwned(form)]));
   }

   public void Own(string form) => Publish(new VocabTevent.Forms.EnteredFormsUpdated(WithTheForm(EnteredForm.Owned(form))));
}
```

- **A domain method decides, and publishes the result; an applier only sets it.** Loading a vocab, which runs no applier,
  and changing it can't disagree.
- **What reaches other vocab leaves the domain call:** a participation handler of `Forms.Added` gets the vocab whose
  question the form is for update, in the same unit of work. Their own `Add` then finds the form there and publishes
  nothing, so it stops, as today's code does.
- **The `Created` tevent reaches the question's applier** although the vocab's root publishes it: a component is routed to
  by its tevents' interfaces, not their classes (`jastudio-rep-opus`). No applier of the same tevent may sit on the root
  as well, or both run.

## The calls, as they stand

1. **Domain tevents apart from `PropertyUpdated`, with appliers on `PropertyUpdated` only** (Magnus). `jastudio-rep-opus`
   agrees, and drew the line: a domain tevent only where something reacts to more than the value, and a rename is one.
2. **A list's `PropertyUpdated` carries the whole new list:** one applier, and every tevent states the result.
3. **Nested tevent interfaces are named without an `I`,** by Compze's convention: `IVocabTevent.Forms.Added`. A file of
   tevent interfaces disables ReSharper's naming inspection after its usings, an exception Magnus approved
   (`.claude/rules/02-universal-local/080-never-suppress-diagnostics.md`).
4. **One file for a taggregate's tevent interfaces, and one for their classes** (Magnus: the whole nested model in one
   file). It is in the shared code standards now, as an exception to one file per type.
5. **A doc comment only where the names don't say it** (Magnus): the convention on the root, why `Renamed` carries the old
   name, why a vocab's wrapper takes any note tevent, and what a text field's tevents are.
6. **Each text field is a place of the shared tomponent `IStringValueTevent`:** one interface and one class per field, and
   a line among the vocab's `Slots`. A vocab has six, the three note types 27 in all. The analyzer with a code fix that
   Magnus has decided on for Compze would write the classes.
7. **No tevent for loading:** a vocab is restored from its stored data and publishes nothing, as Magnus decided.

## Not designed here yet

Deleting a note (Compze has `ITaggregateDeletedTevent`); the tags every note has (`NoteTags`, at the level of
`INoteTevent`); the vocab's other parts; the kanji and the sentences. They follow the same pattern.
