using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// A study-note as the spaced repetition system has it (<see cref="ISpacedRepetitionSystemStudyNotes"/>): its id there, and its fields by name.
/// </summary>
public sealed class StudyNoteInTheSpacedRepetitionSystem
{
   //review-naming:studying: StudyNoteInTheSpacedRepetitionSystem was StudyNoteInAnki, and its StudyNoteId was AnkiNoteId (Claude, 2026-11-15).
   public StudyNoteInTheSpacedRepetitionSystem(long studyNoteId, IReadOnlyDictionary<string, string> fields)
   {
      StudyNoteId = studyNoteId;
      Fields = fields;
   }

   /// <summary>
   /// The study-note's id in the spaced repetition system, in Anki the id of its Anki note, which <see cref="StudyNoteIds"/> maps to the id of the<br/>
   /// note it belongs to.
   /// </summary>
   public long StudyNoteId { get; }

   /// <summary>
   /// Every field of the study-note, those JAStudio doesn't write included, by the field's name in the spaced repetition system: in Anki, such as<br/>
   /// <see cref="AnkiFieldNames.Question"/>.
   /// </summary>
   public IReadOnlyDictionary<string, string> Fields { get; }

   /// <summary>The fields JAStudio would write for <paramref name="note"/> now (<see cref="JPNote.StudyNoteFields"/>) whose value in this study-note is another.</summary>
   /// <remarks>Text is compared as Anki stores it, in Unicode normalization form C, so that a difference is one writing would remove.</remarks>
   public IReadOnlyList<StudyNoteFieldDifference> FieldsThatDifferFromTheStudyNoteOf(JPNote note) =>
   [
      .. note.StudyNoteFields()
             .Where(it => !HasTheField(it.Key, it.Value))
             .Select(it => new StudyNoteFieldDifference(it.Key, Fields.GetValueOrDefault(it.Key), it.Value))
   ];

   bool HasTheField(string name, string value) => Fields.TryGetValue(name, out var valueHere) && AsAnkiStoresIt(valueHere) == AsAnkiStoresIt(value);

   static string AsAnkiStoresIt(string text) => text.Normalize(NormalizationForm.FormC);
}
