using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.Anki;
using JAStudio.Core.Note;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>
/// A study-note as the SRS, the spaced repetition system, has it (<see cref="ISRSStudyNotes"/>): its id there, the id of the note it belongs to,<br/>
/// its fields by name, the tags the student gave it, and the studying status of each of its cards.
/// </summary>
/// <remarks>
/// review-naming:studying: StudyNoteInTheSRS was StudyNoteInAnki, and its StudyNoteId was AnkiNoteId (Claude, 2026-11-15). NoteId, Tags and<br/>
/// CardStudyingStatuses are new the same day: they are what SRSData, which was BackendData, held of every study-note.
/// </remarks>
public sealed class StudyNoteInTheSRS
{
   public StudyNoteInTheSRS(long studyNoteId,
                            NoteId? noteId,
                            IReadOnlyDictionary<string, string> fields,
                            IReadOnlyList<string> tags,
                            IReadOnlyList<CardStudyingStatus> cardStudyingStatuses)
   {
      StudyNoteId = studyNoteId;
      NoteId = noteId;
      Fields = fields;
      Tags = tags;
      CardStudyingStatuses = cardStudyingStatuses;
   }

   /// <summary>
   /// The study-note's id in the spaced repetition system, in Anki the id of its Anki note, which <see cref="StudyNoteIds"/> maps to the id of the<br/>
   /// note it belongs to.
   /// </summary>
   public long StudyNoteId { get; }

   /// <summary>
   /// The id of the note it belongs to, which the study-note itself holds: in Anki, in its field <see cref="AnkiFieldNames.JasNoteId"/>. Null when it<br/>
   /// holds no note's id, as no study-note JAStudio wrote does. The collection needn't have a note with the id: the note may have been deleted<br/>
   /// with its study-note kept.
   /// </summary>
   public NoteId? NoteId { get; }

   /// <summary>
   /// Every field of the study-note, those JAStudio doesn't write included, by the field's name in the spaced repetition system: in Anki, such as<br/>
   /// <see cref="AnkiFieldNames.Question"/>.
   /// </summary>
   public IReadOnlyDictionary<string, string> Fields { get; }

   /// <summary>
   /// The tags the student gave the study-note in the spaced repetition system, as it names them, such as Anki's <c>marked</c>: what its note's<br/>
   /// <see cref="JPNote.AnkiTags"/> are read from.
   /// </summary>
   public IReadOnlyList<string> Tags { get; }

   /// <summary>The studying status of each of the study-note's cards.</summary>
   public IReadOnlyList<CardStudyingStatus> CardStudyingStatuses { get; }

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
