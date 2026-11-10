using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Mcp.Tools.StudyNotes;

/// <summary>A tag that differs between study-notes in Anki and what JAStudio would write, and in how many of the study-notes compared.</summary>
public record StudyNoteTagCount(string Tag, int StudyNotes)
{
   /// <summary>Each tag among <paramref name="tagsOfTheStudyNotes"/>, ignoring case as Anki does, with how many times it's there, the most frequent first.</summary>
   public static IReadOnlyList<StudyNoteTagCount> Of(IEnumerable<string> tagsOfTheStudyNotes) =>
   [
      .. tagsOfTheStudyNotes.GroupBy(it => it, StringComparer.OrdinalIgnoreCase)
                            .Select(it => new StudyNoteTagCount(it.Key, it.Count()))
                            .OrderByDescending(it => it.StudyNotes)
                            .ThenBy(it => it.Tag, StringComparer.Ordinal)
   ];
}
