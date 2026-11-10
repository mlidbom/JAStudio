using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A study-note as Anki has it (<see cref="IAnkiStudyNotes"/>): the id of its Anki note, and its fields by name.</summary>
public sealed class StudyNoteInAnki
{
   public StudyNoteInAnki(long ankiNoteId, IReadOnlyDictionary<string, string> fields)
   {
      AnkiNoteId = ankiNoteId;
      Fields = fields;
   }

   /// <summary>The id of the Anki note, which <see cref="ExternalNoteIdMap"/> maps to the id of the corpus note it belongs to.</summary>
   public long AnkiNoteId { get; }

   /// <summary>Every field of the Anki note, those JAStudio doesn't write included, by the field's name in Anki, such as <see cref="AnkiFieldNames.Question"/>.</summary>
   public IReadOnlyDictionary<string, string> Fields { get; }

   /// <summary>The fields JAStudio would write for <paramref name="note"/> now (<see cref="JPNote.StudyNoteFields"/>) whose value in this study-note is another.</summary>
   /// <remarks>Text is compared as Anki stores it, in Unicode normalization form C, so that a difference is one writing would remove.</remarks>
   public IReadOnlyList<StudyNoteFieldDifference> FieldsThatDifferFromTheStudyNoteOf(JPNote note) =>
   [
      .. note.StudyNoteFields()
             .Where(it => !HasTheField(it.Key, it.Value))
             .Select(it => new StudyNoteFieldDifference(it.Key, Fields.GetValueOrDefault(it.Key), it.Value))
   ];

   bool HasTheField(string name, string value) => Fields.TryGetValue(name, out var inAnki) && AsAnkiStoresIt(inAnki) == AsAnkiStoresIt(value);

   static string AsAnkiStoresIt(string text) => text.Normalize(NormalizationForm.FormC);
}
