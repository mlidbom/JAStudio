# The structure of the notes' tevents: for Magnus to look at

> **Status: step 6 of [the migration](the-migration.md), 2026-11-16, by `taggregates-opus`.** Interfaces and their
> classes only, compiled against Compze's spike at step 1. Nothing publishes or subscribes to them yet. Branch
> `migrating-the-notes-to-the-state-store`, in `src/src_dotnet/JAStudio.Core/`: `Note/Tevents/`,
> `Note/NoteFields/Tevents/` and `Note/Vocabulary/Tevents/`.

## The headline

**A note's tevents mirror the note's parts.** Every note tevent is an `INoteTevent`, and each note class has a root of its
own below it, such as `IVocabTevent`. Each part of a note that changes has a group below its note's root, and the group
is itself a tevent, such as `IVocabTevent.Forms`. The part's tevents are in its group, such as `IVocabTevent.Forms.Added`.
A part that several notes have, such as a text field a person edits, is a shared tomponent with a hierarchy of its own,
`IStringValueTevent`, which each place it has in a note adopts under the place's name: `IVocabTevent.Mnemonic<T>`.

So a subscriber says exactly what it follows:

```csharp
//Any change to any vocab's forms: what the vocab matches in sentences changed.
observe.ForTevent((IVocabTevent.Forms it) => ...);

//A form added to a vocab: the vocab whose question it is get this vocab's question in turn, in the same unit of work.
handle.ForTevent((IVocabTevent.Forms.Added it, ITaggregateStoreUpdater notes) => ...);

//A vocab's mnemonic set, and nothing else about it.
observe.ForTevent((IVocabTevent.Mnemonic<IStringValueTevent.Set> it) => ...);

//Anything at all about any note.
observe.ForTevent((INoteTevent it) => ...);
```

## The parts of the structure

| What | Interface | Class |
|---|---|---|
| Every note's root | `INoteTevent : ITaggregateTevent` | `NoteTevent` |
| A note class's root | `IVocabTevent : INoteTevent` | `VocabTevent` |
| A new note | `IVocabTevent.Created : ITaggregateCreatedTevent, Question.TextChanged` | `VocabTevent.Created` |
| A part's group | `IVocabTevent.Forms : IVocabTevent` | none: never published itself |
| A part's tevent | `IVocabTevent.Forms.Added { Form }` | `VocabTevent.Forms.Added` |
| A shared tomponent's own tevents | `IStringValueTevent : ITevent`, `IStringValueTevent.Set { Value }` | `StringValueTevent.Set` |
| A shared tomponent's place in a note | `IVocabTevent.Mnemonic<out T> : IVocabTevent, IPublisherTevent<T>` | `VocabTevent.Mnemonic<T>` |
| The wrapper Compze puts around each tevent a note publishes | `INoteTevent<out T>`, `IVocabTevent<out T>` | `NoteTevent<T>`, `VocabTevent<T>` |

**Written so far:** the roots, the vocab's creation, its question (`TextChanged`, `DisambiguationChanged`), its forms
(`Added`, `Removed`, `Owned`, `NoLongerOwned`, `Replaced`), and its six text fields' places: `CorpusAnswer`,
`Explanation`, `ExplanationLong`, `Mnemonic`, `TechnicalNotes` and `References`.

## How `VocabNoteForms` will publish and apply them

A sketch, not compiled: how a component declares its tevent appliers comes with Compze's step 3, and its names are
placeholders until the naming pass. What it shows is the shape: a domain method decides and publishes, and an applier
only changes state, so loading a vocab, which runs no applier, and changing it can't disagree.

```csharp
public class VocabNoteForms : VocabNote.Component<IVocabTevent.Forms>
{
   List<EnteredForm> _enteredForms;

   static readonly TeventRouting<VocabNoteForms, IVocabTevent.Forms> Appliers = DeclareAppliers()
      .For<IVocabTevent.Forms.Added>((forms, it) => forms._enteredForms.Add(EnteredForm.NotOwned(it.Form)))
      .For<IVocabTevent.Forms.Removed>((forms, it) => forms._enteredForms.RemoveAll(form => form.Text == it.Form))
      .For<IVocabTevent.Forms.Owned>((forms, it) => forms.Replace(EnteredForm.Owned(it.Form)))
      .For<IVocabTevent.Forms.NoLongerOwned>((forms, it) => forms.Replace(EnteredForm.NotOwned(it.Form)))
      .For<IVocabTevent.Forms.Replaced>((forms, it) => forms._enteredForms = [.. it.EnteredForms]);

   public void Add(string form)
   {
      if(IsTheQuestion(form) || EnteredFormsSet().Contains(form)) return; //A tevent is published only when something changes.
      Publish(new VocabTevent.Forms.Added(form));
   }
}
```

- **What reaches other vocab leaves the domain call.** Today `Add` also adds this vocab's question to the forms of the vocab
  whose question the form is, by calling them. With tevents that is a participation handler of `IVocabTevent.Forms.Added`,
  which gets those vocab for update in the same unit of work. Their own `Add` then finds the form there and publishes
  nothing, so it stops, as today's code does.
- **A part's reaction to another part becomes a tevent of its own.** Today `VocabNoteQuestion.SetText` calls
  `VocabNoteForms.OnQuestionChanged`, which quietly drops the new question from the entered forms. With tevents, setting
  the question publishes `Question.TextChanged`, and, when the new question was an entered form,
  `Forms.Removed`, so that what follows the forms sees that change too.

## The calls I made, for you to check

1. **Nested tevent interfaces are named without an `I`,** by Compze's semantic-events convention: `IVocabTevent.Forms.Added`
   reads as what happened. ReSharper's naming rule flags each at suggestion level (`InconsistentNaming`, rule
   `Interfaces`). I suppressed nothing. The alternatives: leave the suggestions; disable the inspection at the top of the
   tevent files, as the approved exception does for BDD specification files; or prefix them, `IVocabTevent.IForms.IAdded`.
   I recommend the file-level disable, as Compze's own tevent files do.
2. **A part's group is a tevent itself,** which a subscriber can follow, not a static class as the group
   `PropertyUpdated` is in Compze's sample. Following "anything about the forms" is the commonest need, such as for
   reparsing.
3. **The forms' tevents are granular, plus `Replaced` for an edit of the whole list,** as a person makes in the editor.
   Turning that edit into `Added`, `Removed` and `Owned` would lose the order of the entered forms, which the vocab
   keeps, and wouldn't be what happened.
4. **Each text field is a place of the shared tomponent `IStringValueTevent`,** with a wrapper of its own: one interface
   and one class per field, two files of a few lines each. A vocab has six such fields, and the three note types 27 in all.
   The analyzer with a code fix that Magnus has decided on for Compze would write the classes.
5. **One file for each nested type,** as the code standards say: 22 files so far, most of them a few lines.
6. **`Created` is also a `Question.TextChanged`,** as the sample's `UserRegistered` is also a `PropertyUpdated.Email`: what
   follows a vocab's question follows a new vocab's question too.
7. **No tevent for loading:** a vocab is restored from its stored data and publishes nothing, as Magnus decided.

## Not designed here yet

Deleting a note (Compze has `ITaggregateDeletedTevent`); the tags every note has (`NoteTags`, at the level of
`INoteTevent`); the vocab's other parts; the kanji and the sentences. They follow the same pattern, and wait for your
look at this one.
