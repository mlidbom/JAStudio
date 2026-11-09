using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// A vocab's parts of speech (<see cref="VocabNotePartsOfSpeech"/>), by their names, each one of the <see cref="PartOfSpeech.All"/> JAStudio knows.<br/>
/// Its value is the names in alphabetical order. Every change changes how sentences parse, such as whether the vocab inflects as a godan verb.
/// </summary>
public sealed class EditablePartsOfSpeechField : EditableField<VocabNote, IReadOnlyList<string>>
{
   public EditablePartsOfSpeechField() : base("partsOfSpeech", "Parts of speech", changesHowSentencesParse: true) {}

   public override IReadOnlyList<string> Read(VocabNote note) => [.. note.PartsOfSpeech.All().Select(it => it.Name)];

   /// <summary>Replaces the vocab's parts of speech with those named in <paramref name="value"/>, or, when one is unknown, changes nothing and throws.</summary>
   public override void Write(VocabNote note, IReadOnlyList<string> value) => note.PartsOfSpeech.Set([.. value.Select(PartOfSpeechNamed)]);

   static PartOfSpeech PartOfSpeechNamed(string name) =>
      PartOfSpeech.Exists(name)
         ? PartOfSpeech.Named(name)
         : throw new ArgumentException($"There is no part of speech named '{name}'. The parts of speech JAStudio knows are: {string.Join(", ", PartOfSpeech.All.Select(it => $"\"{it.Name}\""))}.");
}
