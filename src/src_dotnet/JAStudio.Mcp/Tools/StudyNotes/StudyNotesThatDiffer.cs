using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki.StudyNotes;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>What <see cref="StudyNoteTools.ListTheStudyNotesThatDiffer"/> reports of a <see cref="StudyNoteComparison"/>.</summary>
/// <param name="StudyNotesCompared">How many study-notes were compared: those whose note the collection has.</param>
/// <param name="Fields">Each field that differs in some study-note, with how many it differs in.</param>
/// <param name="StudyNotesOfNoNote">The study-notes in Anki that belong to no note of the collection (<see cref="StudyNoteComparison.StudyNotesOfNoNote"/>), every one.</param>
public record StudyNotesThatDiffer(
   int StudyNotesCompared,
   IReadOnlyList<StudyNoteFieldDifferenceCount> Fields,
   StudyNotesThatDifferOfOneType Vocab,
   StudyNotesThatDifferOfOneType Kanji,
   StudyNotesThatDifferOfOneType Sentences,
   IReadOnlyList<StudyNoteOfNoNote> StudyNotesOfNoNote)
{
   /// <summary>
   /// The report of <paramref name="comparison"/>, listing at most <paramref name="maxNotesListedPerType"/> notes of each type, and none that<br/>
   /// <paramref name="isLeftOutOfTheList"/>.
   /// </summary>
   public static StudyNotesThatDiffer In(StudyNoteComparison comparison, int maxNotesListedPerType, Func<NoteWhoseStudyNoteDiffers, bool> isLeftOutOfTheList)
   {
      var differing = comparison.NotesWhoseStudyNoteDiffers;
      return new StudyNotesThatDiffer(
         comparison.NotesCompared,
         [
            .. differing.SelectMany(it => it.Fields)
                        .GroupBy(it => it.Field)
                        .Select(it => new StudyNoteFieldDifferenceCount(it.Key, it.Count()))
                        .OrderByDescending(it => it.StudyNotes)
         ],
         StudyNotesThatDifferOfOneType.Of([.. differing.Where(it => it.Note is VocabNote)], maxNotesListedPerType, isLeftOutOfTheList),
         StudyNotesThatDifferOfOneType.Of([.. differing.Where(it => it.Note is KanjiNote)], maxNotesListedPerType, isLeftOutOfTheList),
         StudyNotesThatDifferOfOneType.Of([.. differing.Where(it => it.Note is SentenceNote)], maxNotesListedPerType, isLeftOutOfTheList),
         [.. comparison.StudyNotesOfNoNote.Select(StudyNoteOfNoNote.Of)]);
   }
}
