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
/// <param name="StudyNotesWhoseTagsDiffer">How many study-notes have tags other than those JAStudio would write.</param>
/// <param name="TagsOnlyInAnki">Each tag some study-note has in Anki that JAStudio wouldn't write, with how many study-notes have it.</param>
/// <param name="TagsOnlyJAStudioWrites">Each tag JAStudio would write that some study-note hasn't in Anki, with how many study-notes lack it.</param>
/// <param name="StudyNotesOfNoNote">How many study-notes in Anki belong to no note of the collection (<see cref="StudyNoteComparison.StudyNotesOfNoNote"/>).</param>
public record StudyNotesThatDiffer(
   int StudyNotesCompared,
   IReadOnlyList<StudyNoteFieldDifferenceCount> Fields,
   int StudyNotesWhoseTagsDiffer,
   IReadOnlyList<StudyNoteTagCount> TagsOnlyInAnki,
   IReadOnlyList<StudyNoteTagCount> TagsOnlyJAStudioWrites,
   StudyNotesThatDifferOfOneType Vocab,
   StudyNotesThatDifferOfOneType Kanji,
   StudyNotesThatDifferOfOneType Sentences,
   int StudyNotesOfNoNote)
{
   /// <summary>The report of <paramref name="comparison"/>, listing at most <paramref name="maxNotesListedPerType"/> notes of each type.</summary>
   public static StudyNotesThatDiffer In(StudyNoteComparison comparison, int maxNotesListedPerType)
   {
      var differing = comparison.NotesWhoseStudyNoteDiffers;
      return new StudyNotesThatDiffer(
         comparison.NotesCompared,
         [
            .. differing.SelectMany(it => it.Differences.Fields)
                        .GroupBy(it => it.Field)
                        .Select(it => new StudyNoteFieldDifferenceCount(it.Key, it.Count()))
                        .OrderByDescending(it => it.StudyNotes)
         ],
         differing.Count(it => it.Differences.TagsDiffer),
         StudyNoteTagCount.Of(differing.SelectMany(it => it.Differences.TagsOnlyInAnki)),
         StudyNoteTagCount.Of(differing.SelectMany(it => it.Differences.TagsOnlyJAStudioWrites)),
         StudyNotesThatDifferOfOneType.Of(differing.Where(it => it.Note is VocabNote), maxNotesListedPerType),
         StudyNotesThatDifferOfOneType.Of(differing.Where(it => it.Note is KanjiNote), maxNotesListedPerType),
         StudyNotesThatDifferOfOneType.Of(differing.Where(it => it.Note is SentenceNote), maxNotesListedPerType),
         comparison.StudyNotesOfNoNote.Count);
   }
}
