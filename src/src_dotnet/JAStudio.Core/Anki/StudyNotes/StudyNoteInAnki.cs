using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JAStudio.Core.Note;

namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A study-note as Anki has it (<see cref="IAnkiStudyNotes"/>): the id of its Anki note, its fields by name, and its tags.</summary>
public sealed class StudyNoteInAnki
{
   public StudyNoteInAnki(long ankiNoteId, IReadOnlyDictionary<string, string> fields, IReadOnlyList<string> tags)
   {
      AnkiNoteId = ankiNoteId;
      Fields = fields;
      Tags = tags;
   }

   /// <summary>The id of the Anki note, which <see cref="ExternalNoteIdMap"/> maps to the id of the corpus note it belongs to.</summary>
   public long AnkiNoteId { get; }

   /// <summary>Every field of the Anki note, those JAStudio doesn't write included, by the field's name in Anki, such as <see cref="AnkiFieldNames.Question"/>.</summary>
   public IReadOnlyDictionary<string, string> Fields { get; }

   public IReadOnlyList<string> Tags { get; }

   /// <summary>How this study-note differs from the one JAStudio would write for <paramref name="note"/> now (<see cref="JPNote.GetData"/>).</summary>
   /// <remarks>
   /// It compares as Anki keeps what's written into it, so that a difference is one writing would remove: text as Anki stores it, in Unicode<br/>
   /// normalization form C, and tags ignoring case and order, since Anki gives a tag the case of a tag it already has, and sorts them.
   /// </remarks>
   public StudyNoteDifferences DifferencesFromTheStudyNoteOf(JPNote note)
   {
      var written = note.GetData();
      var tagsInAnki = Tags.ToHashSet(StringComparer.OrdinalIgnoreCase);
      var tagsWritten = written.Tags.ToHashSet(StringComparer.OrdinalIgnoreCase);

      return new StudyNoteDifferences(
         [.. written.Fields.Where(it => !HasTheField(it.Key, it.Value)).Select(it => new StudyNoteFieldDifference(it.Key, Fields.GetValueOrDefault(it.Key), it.Value))],
         [.. Tags.Where(it => !tagsWritten.Contains(it))],
         [.. written.Tags.Where(it => !tagsInAnki.Contains(it))]);

      bool HasTheField(string name, string value) => Fields.TryGetValue(name, out var inAnki) && AsAnkiStoresIt(inAnki) == AsAnkiStoresIt(value);

      static string AsAnkiStoresIt(string text) => text.Normalize(NormalizationForm.FormC);
   }
}
