using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Mcp.Tools.Corpus.NoteDiffs.Properties;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs;

/// <summary>How the notes of one type differ between two versions of the corpus: how many changed in each way, which properties, and the first notes' diffs.</summary>
/// <param name="ChangedOnlyInTheirFiles">How many notes' files differ while the notes read from them are the same: <see cref="NoteChangeKind.ChangedOnlyInItsFile"/>.</param>
/// <param name="PropertiesChanged">Each property that changed, with how many notes it changed in, the most first.</param>
/// <param name="Notes">The diffs of the first notes by question, at most as many as asked for: notes changed only in their files aren't listed.</param>
public record NotesDiffOfOneType(
   int Changed,
   int Added,
   int Deleted,
   int ChangedOnlyInTheirFiles,
   IReadOnlyList<PropertyChangeCount> PropertiesChanged,
   IReadOnlyList<NoteDiff> Notes)
{
   public static NotesDiffOfOneType Of(IReadOnlyList<NoteDiff> diffs, int maxNotesListed) =>
      new(diffs.Count(it => it.Change == NoteChangeKind.Changed),
          diffs.Count(it => it.Change == NoteChangeKind.Added),
          diffs.Count(it => it.Change == NoteChangeKind.Deleted),
          diffs.Count(it => it.Change == NoteChangeKind.ChangedOnlyInItsFile),
          [.. diffs.SelectMany(it => it.PropertiesChanged())
                   .CountBy(it => it)
                   .Select(it => new PropertyChangeCount(it.Key, it.Value))
                   .OrderByDescending(it => it.Notes)
                   .ThenBy(it => it.Property, StringComparer.Ordinal)],
          [.. diffs.Where(it => it.Change != NoteChangeKind.ChangedOnlyInItsFile)
                   .OrderBy(it => it.Question, StringComparer.Ordinal)
                   .Take(maxNotesListed)]);
}
