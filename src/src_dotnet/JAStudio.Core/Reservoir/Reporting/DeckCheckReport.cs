using System.Collections.Generic;
using System.IO;
using System.Linq;
using JAStudio.Core.Reporting;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;
using static System.FormattableString;

namespace JAStudio.Core.Reservoir.Reporting;

/// <summary>
/// The report of what each deck in a folder would give the reservoir (<see cref="WhatADeckGives"/>), written before filling the reservoir from it,<br/>
/// to check the fields recognized as the Japanese, the translation and the media: <c>report.md</c> lists the note types to look at, and<br/>
/// <c>note-types.tsv</c> every note type, with examples of its sentences.
/// </summary>
public sealed class DeckCheckReport
{
   readonly IReadOnlyList<WhatADeckGives> _decks;
   readonly ReportFolder _folder;

   DeckCheckReport(IReadOnlyList<WhatADeckGives> decks, ReportFolder folder)
   {
      _decks = decks;
      _folder = folder;
   }

   /// <summary>Every deck's note types that have notes.</summary>
   IEnumerable<WhatANoteTypeGives> NoteTypes => _decks.SelectMany(it => it.NoteTypes);

   /// <summary>Writes the report of <paramref name="decks"/>, the decks in <paramref name="decksFolder"/>, into <paramref name="reportFolder"/>, created if need be.</summary>
   public static void WriteTo(DirectoryInfo reportFolder, DirectoryInfo decksFolder, IReadOnlyList<WhatADeckGives> decks)
   {
      var report = new DeckCheckReport(decks, new ReportFolder(reportFolder));
      report._folder.Title($"What the decks in {decksFolder.FullName} would give the reservoir");
      report.WriteTheTotals();
      report.WriteTheFieldsRecognizedAsTheJapanese();
      report.WriteTheNoteTypesGivingNothing();
      report.WriteTheNoteTypesGivingWords();
      report.WriteEveryNoteType();
      report._folder.SaveTheMarkdown();
   }

   void WriteTheTotals() =>
      _folder.Section("Totals",
                      Invariant($"{_decks.Count:N0} decks, with {NoteTypes.Count():N0} note types that have notes"),
                      Invariant($"{NoteTypes.Sum(it => it.NoteCount):N0} notes, giving {_decks.Sum(it => it.SentenceCount):N0} sentences"),
                      Invariant($"{_decks.Count(it => it.SentenceCount == 0):N0} decks give no sentence at all"));

   /// <summary>How many note types, and sentences, each field name was recognized as the Japanese in: a name that doesn't fit, such as a reading's, is to be looked at.</summary>
   void WriteTheFieldsRecognizedAsTheJapanese()
   {
      _folder.Section("The fields recognized as the Japanese, by name");
      _folder.TableWithTheRestInAFile("japanese-fields",
                                      ["Field", "Note types", "Sentences"],
                                      [.. NoteTypes.Where(it => it.Roles.JapaneseField is not null)
                                                   .GroupBy(it => it.Roles.JapaneseField)
                                                   .OrderByDescending(it => it.Count())
                                                   .Select(it => new[] { it.Key ?? "", Invariant($"{it.Count():N0}"), Invariant($"{it.Sum(noteType => noteType.SentenceCount):N0}") })]);
   }

   void WriteTheNoteTypesGivingNothing()
   {
      var givingNothing = NoteTypes.Where(it => it.GivesNothingFromItsNotes).OrderByDescending(it => it.NoteCount).ToList();
      _folder.Section("Note types with notes that give no sentence",
                      Invariant($"{givingNothing.Count:N0} note types, with {givingNothing.Sum(it => it.NoteCount):N0} notes: no field was recognized as the Japanese, or the one that was holds none."));
      _folder.TableWithTheRestInAFile("note-types-giving-nothing",
                                      ["Deck", "Note type", "Notes", "Fields"],
                                      [.. givingNothing.Select(it => new[] { it.Deck.Name, it.NoteType.Name, Invariant($"{it.NoteCount:N0}"), FieldNamesOf(it) })]);
   }

   void WriteTheNoteTypesGivingWords()
   {
      var givingWords = NoteTypes.Where(it => it.GivesWordsRatherThanSentences).OrderByDescending(it => it.TextCount).ToList();
      _folder.Section("Note types giving words rather than sentences",
                      Invariant($"{givingWords.Count:N0} note types, with {givingWords.Sum(it => it.TextCount):N0} notes whose Japanese is so short that it's most likely words, as a vocabulary deck's is: they give no sentences."));
      _folder.TableWithTheRestInAFile("note-types-giving-words",
                                      ["Deck", "Note type", "Notes with Japanese", "Average length", "Japanese field", "Examples", "Fields"],
                                      [.. givingWords.Select(Row)]);
      return;

      static string[] Row(WhatANoteTypeGives it) =>
      [
         it.Deck.Name, it.NoteType.Name, Invariant($"{it.TextCount:N0}"), Invariant($"{it.AverageTextLength:F1}"), it.Roles.JapaneseField ?? "", ExamplesOf(it), FieldNamesOf(it)
      ];
   }

   void WriteEveryNoteType()
   {
      _folder.Section("Every note type", "In `note-types.tsv`: each deck's note types, with the fields recognized in them, what they give, and examples of their sentences.");
      _folder.TabSeparatedFile("note-types",
                               ["Deck", "Note type", "Notes", "Sentences", "Average length", "Japanese field", "Its furigana taken away", "Words, not sentences", "Translation field", "Audio field", "Image field", "Examples", "Fields"],
                               NoteTypes.Select(Row));
      return;

      static string[] Row(WhatANoteTypeGives it) =>
      [
         it.Deck.Name, it.NoteType.Name, Invariant($"{it.NoteCount}"), Invariant($"{it.SentenceCount}"), Invariant($"{it.AverageTextLength:F1}"),
         it.Roles.JapaneseField ?? "", YesOrEmpty(it.Roles.JapaneseFieldHoldsFurigana), YesOrEmpty(it.GivesWordsRatherThanSentences),
         it.Roles.TranslationField ?? "", it.Roles.AudioField ?? "", it.Roles.ImageField ?? "", ExamplesOf(it), FieldNamesOf(it)
      ];

      static string YesOrEmpty(bool yes) => yes ? "yes" : "";
   }

   /// <summary>The examples of <paramref name="noteType"/>'s Japanese, separated by slashes.</summary>
   static string ExamplesOf(WhatANoteTypeGives noteType) => string.Join(" / ", noteType.Examples);

   /// <summary>The names of <paramref name="noteType"/>'s fields, in their order, separated by commas.</summary>
   static string FieldNamesOf(WhatANoteTypeGives noteType) => string.Join(", ", noteType.NoteType.FieldNames);
}
