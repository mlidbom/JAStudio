using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JAStudio.Core.Note.Collection;
using ModelContextProtocol.Server;

namespace JAStudio.Mcp.Tools.Copies;

/// <summary>The tools about the copies of the notes that a taggregate store makes for a unit of work, through each note's snapshot.</summary>
[McpServerToolType]
public partial class CopyTools
{
   readonly JPCollection _collection;

   public CopyTools(JPCollection collection) => _collection = collection;

   [McpServerTool(Name = "find_what_copies_of_the_notes_lose", ReadOnly = true, Idempotent = true)]
   [Description("""
                Copies every note through its snapshot, its DTO, as a taggregate store makes the copy a unit of work changes, and compares the note
                and its copy field by field, in every part, and their snapshots, with Compze's SnapshotRoundTrip. What a copy loses is lost at the
                note's next edit. Reports how many notes it checked and how long that took, and each field that differs, by its path with the
                positions in lists left out, such as VocabNote.Forms._forms[]: how many notes it differs in, and one difference as an example.
                """)]
   public WhatCopiesLose FindWhatCopiesOfTheNotesLose()
   {
      var stopwatch = Stopwatch.StartNew();
      var notesChecked = _collection.Vocab.All().Count + _collection.Kanji.All().Count + _collection.Sentences.All().Count;
      var differing = CopiesOfTheNotes.ThatDifferIn(_collection);

      var fields = differing.SelectMany(copy => copy.Differences.Distinct().Select(difference => (Field: TheFieldOf(difference), Difference: difference, copy.Note)))
                            .GroupBy(it => it.Field)
                            .Select(field => new AFieldACopyLoses(field.Key,
                                                                  field.Select(it => it.Note).Distinct().Count(),
                                                                  $"{field.First().Note.GetQuestion()}: {field.First().Difference}"))
                            .OrderByDescending(it => it.Notes)
                            .ToList();

      return new WhatCopiesLose(notesChecked, differing.Count, stopwatch.Elapsed.TotalSeconds, fields);
   }

   /// <summary>The path of the field a difference names, the text before its first <c>": "</c>, with each position in a list left out, as <c>[]</c>.</summary>
   static string TheFieldOf(string difference)
   {
      var endOfThePath = difference.IndexOf(": ", System.StringComparison.Ordinal);
      return APositionInAList().Replace(endOfThePath < 0 ? difference : difference[..endOfThePath], "[]");
   }

   [GeneratedRegex(@"\[[^\]]*\]")]
   private static partial Regex APositionInAList();
}
