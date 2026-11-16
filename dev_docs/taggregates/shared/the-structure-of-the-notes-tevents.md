# The structure of the notes' tevents

> **Status: step 6 of [the migration](the-migration.md), revised 2026-11-16 after Magnus's look, with `jastudio-rep-opus`.**
> Interfaces and their classes only. Nothing publishes or subscribes to them yet. Branch
> `migrating-the-notes-to-the-state-store`, in `src/src_dotnet/JAStudio.Core/`: `Note/Tevents/` and
> `Note/Vocabulary/Tevents/`, a file with every interface of a taggregate's tevents and one with their classes.

## The headline

**A note's tevents mirror the note's data, and keep domain tevents apart from `PropertyUpdated` tevents** (Magnus,
2026-11-16).

- **Every note tevent is an `INoteTevent`,** and each note class has a root of its own below it, such as `IVocabTevent`.
- **A `PropertyUpdated` tevent carries one property's whole new value:** `IVocabTevent.PropertyUpdated.Mnemonic`,
  `IVocabTevent.Forms.PropertyUpdated.EnteredForms`. The tevent appliers apply only these, so however many domain tevents
  later set a property, no applier changes.
- **A part of a note that holds more than one property, or has behavior of its own, has a group,** which is itself a
  tevent: `IVocabTevent.Forms`, `IVocabTevent.Question`. A note's plain properties, such as its text fields, are
  updated at the note's root.
- **A domain tevent says what happened beyond the value,** and is also the `PropertyUpdated` tevent it causes:
  `IVocabTevent.Forms.Added : PropertyUpdated.EnteredForms`, with the form added and the whole resulting list.
- **A domain tevent exists only where something reacts to more than the new value.** This domain is mostly edits of
  values, so most tevents are `PropertyUpdated` only, and honestly so. The tevents aren't stored, so a domain tevent can be
  added the day something needs it.

So a subscriber says exactly what it follows:

```csharp
//Any change to any vocab's forms: what the vocab matches in sentences changed.
observe.ForTevent((IVocabTevent.Forms it) => ...);

//A form added: the vocab whose question it is get this vocab's question in turn, in the same unit of work.
handle.ForTevent((IVocabTevent.Forms.Added it, ITaggregateStoreUpdater notes) => ...);

//A vocab renamed: the word-references to its old name, in other notes, are renamed.
handle.ForTevent((IVocabTevent.Question.Renamed it, ITaggregateStoreUpdater notes) => ...);

//A vocab's mnemonic, and nothing else about it.
observe.ForTevent((IVocabTevent.PropertyUpdated.Mnemonic it) => ...);
```

## The vocab's tevents so far

```
IVocabTevent : INoteTevent
   PropertyUpdated.CorpusAnswer, .Explanation, .ExplanationLong, .Mnemonic, .TechnicalNotes, .References
   Created : ITaggregateCreatedTevent, Question.PropertyUpdated.Text
   Question
      PropertyUpdated.Text            { Text }
      PropertyUpdated.Disambiguation  { Disambiguation }
      Renamed : PropertyUpdated.Text, PropertyUpdated.Disambiguation  { From }
   Forms
      PropertyUpdated.EnteredForms    { EnteredForms }
      Added   : PropertyUpdated.EnteredForms  { Form }
      Removed : PropertyUpdated.EnteredForms  { Form }
```

- **The domain tevents, and what reacts to each:**
  - `Created`, a new vocab.
  - `Question.Renamed`: the word-references to the vocab's old name are renamed. It carries the old name because nothing
    else has it: inside a unit of work, the vocab read is its own copy, already renamed. Changing the disambiguation
    renames the vocab too, so `Renamed` sets both properties.
  - `Forms.Added` and `Forms.Removed`: the vocab whose question the form is get or lose this vocab's question in turn.
- **Owning a form, or no longer owning it, is `PropertyUpdated.EnteredForms` only** for now: nothing reacts to more.
- **A text field is a plain string of the vocab,** with its `PropertyUpdated` tevent. Today each of the notes' 27 text fields
  is a `WritableStringValue` object: trimming, changing nothing when the value is equal, and an `IObservable<string>` that
  only `PerfectSynonyms` follows. With tevents each is a line of the setter, or a subscription, so the class goes as the
  fields are converted (Magnus, 2026-11-16). A first version made each field a shared tomponent with a place of its own:
  that copied the old class, and was dropped.

## How the `PropertyUpdated` levels are written

As on Compze's event-naming page, `IUserEvent.IProfile.IPropertyUpdated.IName`, with two differences that naming nested
tevents without an `I` brings:

- **`PropertyUpdated` is a static class, as in Compze's sample,** not an interface. As an interface, `Text : PropertyUpdated`
  would inherit its own name, and its property `Text` would hide it (CS0108).
- **A part's `PropertyUpdated` is declared `new`,** since a nested type is inherited by every interface that extends its
  owner, and the part's hides the note's. A tevent that is both, such as `Created`, then sees two, which ReSharper reports
  on every tevent (`PossibleInterfaceMemberAmbiguity`). Files of tevent interfaces disable that inspection, an exception
  Magnus approved: he suppresses it routinely, and has never met a real problem with it.

## How `VocabNoteForms` will publish and apply them

A sketch, not compiled: how a class declares its tevent appliers comes with Compze's step 3. Its shape is that of
`jastudio-rep-opus`'s experiment, Compze's branch `taggregate-experiments-opus`,
`test/Compze.Tests.Unit/CQRS/Taggregates/PerClassTeventRouting/VocabNote.cs`, and its names are placeholders until the
naming pass.

```csharp
public class VocabNote : JPNote
{
   //The vocab's table extends every note's, applies the vocab's own properties, and names its parts: a part is routed to
   //only when its owner's table names it.
   static readonly TeventRouting<VocabNote> Appliers = NoteAppliers.Extend<VocabNote>(it => it
      .For<IVocabTevent.PropertyUpdated.Mnemonic>(static (vocab, tevent) => vocab.Mnemonic = tevent.Mnemonic)   //and the other five text fields
      .Component(static vocab => vocab.Question)
      .Component(static vocab => vocab.Forms));

   public string Mnemonic { get; private set; } = "";

   public void SetMnemonic(string mnemonic) => PublishIfChanged(Mnemonic, mnemonic.Trim(), static it => new VocabTevent.MnemonicUpdated(it));
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
- **"Publish only when it changed" is written once,** as a helper such as `PublishIfChanged`, not in each of 27 setters.
- **What reaches other vocab leaves the domain call:** a participation handler of `Forms.Added` gets the vocab whose
  question the form is for update, in the same unit of work. Their own `Add` then finds the form there and publishes
  nothing, so it stops, as today's code does.
- **The `Created` tevent reaches the question's applier** although the vocab's root publishes it: a component is routed to
  by its tevents' interfaces, not their classes (`jastudio-rep-opus`). No applier of the same tevent may sit on the root
  as well, or both run.

## The calls, as they stand

1. **Domain tevents apart from `PropertyUpdated` tevents, with appliers on `PropertyUpdated` only** (Magnus). `jastudio-rep-opus`
   agrees, and drew the line: a domain tevent only where something reacts to more than the value, and a rename is one.
2. **A list's `PropertyUpdated` carries the whole new list:** one applier, and every tevent states the result.
3. **Nested tevent interfaces are named without an `I`,** by Compze's convention. A file of tevent interfaces disables
   ReSharper's naming and interface-ambiguity inspections after its usings, exceptions Magnus approved
   (`.claude/rules/02-universal-local/080-never-suppress-diagnostics.md`).
4. **One file for a taggregate's tevent interfaces, and one for their classes** (Magnus). It is in the shared code
   standards now, as an exception to one file per type.
5. **A doc comment only where the names don't say it** (Magnus).
6. **Plain properties, not wrapping objects,** for data that is only a value (Magnus). What else the notes hold is being
   gone through the same way.
7. **No tevent for loading:** a vocab is restored from its stored data and publishes nothing, as Magnus decided.

## Not designed here yet

Deleting a note (Compze has `ITaggregateDeletedTevent`); the tags every note has (`NoteTags`, at the level of
`INoteTevent`); the vocab's other parts; the kanji and the sentences. They follow the same pattern.
