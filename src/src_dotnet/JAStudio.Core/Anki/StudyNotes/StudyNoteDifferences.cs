using System.Collections.Generic;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>How a study-note in Anki differs from the one JAStudio would write for its note now (<see cref="StudyNoteInAnki.DifferencesFromTheStudyNoteOf"/>).</summary>
public sealed class StudyNoteDifferences
{
   public StudyNoteDifferences(IReadOnlyList<StudyNoteFieldDifference> fields, IReadOnlyList<string> tagsOnlyInAnki, IReadOnlyList<string> tagsOnlyJAStudioWrites)
   {
      Fields = fields;
      TagsOnlyInAnki = tagsOnlyInAnki;
      TagsOnlyJAStudioWrites = tagsOnlyJAStudioWrites;
   }

   /// <summary>The fields JAStudio writes whose value in Anki is another.</summary>
   public IReadOnlyList<StudyNoteFieldDifference> Fields { get; }

   /// <summary>The tags the study-note has in Anki that JAStudio wouldn't write.</summary>
   public IReadOnlyList<string> TagsOnlyInAnki { get; }

   /// <summary>The tags JAStudio would write that the study-note hasn't in Anki.</summary>
   public IReadOnlyList<string> TagsOnlyJAStudioWrites { get; }

   public bool TagsDiffer => TagsOnlyInAnki.Count > 0 || TagsOnlyJAStudioWrites.Count > 0;

   public bool Any => Fields.Count > 0 || TagsDiffer;
}
