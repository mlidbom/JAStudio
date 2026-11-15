using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Studying;
using JAStudio.Core.Studying.StudyNotes;

namespace JAStudio.Core.TestUtils;

public partial class PretendAnki
{
   /// <summary>A study-note in the pretend Anki: an Anki note of one of JAStudio's note types, as it is now.</summary>
   sealed class StudyNote
   {
      readonly List<CardStudyingStatus> _suspendedCards = [];

      public StudyNote(long ankiNoteId, NoteId? corpusNoteId, IReadOnlyDictionary<string, string> fields)
      {
         AnkiNoteId = ankiNoteId;
         CorpusNoteId = corpusNoteId;
         Fields = fields;
      }

      public long AnkiNoteId { get; }

      /// <summary>The id of the corpus note it is, or null when it holds no corpus note's id.</summary>
      public NoteId? CorpusNoteId { get; }

      /// <summary>Its fields, by their names in Anki.</summary>
      public IReadOnlyDictionary<string, string> Fields { get; set; }

      /// <summary>Its tags, as Anki names them.</summary>
      public IReadOnlyList<string> Tags { get; set; } = [];

      /// <summary>Suspends its card of <paramref name="cardType"/>, as a study-note of the note type named <paramref name="noteTypeName"/>.</summary>
      public void SuspendTheCard(string cardType, string noteTypeName) => _suspendedCards.Add(new CardStudyingStatus(AnkiNoteId, cardType, isSuspended: true, noteTypeName));

      /// <summary>The study-note as JAStudio reads it from Anki now: a copy, which later changes here don't reach.</summary>
      public StudyNoteInTheSRS AsReadFromAnki() => new(AnkiNoteId, CorpusNoteId, new Dictionary<string, string>(Fields), [.. Tags], [.. _suspendedCards]);
   }
}
