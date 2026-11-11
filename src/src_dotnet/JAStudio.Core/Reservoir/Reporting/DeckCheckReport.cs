using System.Collections.Generic;
using System.IO;
using System.Linq;
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

   IEnumerable<(WhatADeckGives Deck, WhatANoteTypeGives NoteType)> NoteTypes => _decks.SelectMany(deck => deck.NoteTypes.Select(noteType => (deck, noteType)));

   /// <summary>Writes the report of <paramref name="decks"/>, the decks in <paramref name="decksFolder"/>, into <paramref name="folder"/>, created if need be.</summary>
   public static void WriteTo(DirectoryInfo folder, DirectoryInfo decksFolder, IReadOnlyList<WhatADeckGives> decks)
   {
      var report = new DeckCheckReport(decks, new ReportFolder(folder));
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
                      Invariant($"{NoteTypes.Sum(it => it.NoteType.NoteCount):N0} notes, giving {_decks.Sum(it => it.SentenceCount):N0} sentences"),
                      Invariant($"{_decks.Count(it => it.SentenceCount == 0):N0} decks give no sentence at all"));

   /// <summary>How many note types, and sentences, each field name was recognized as the Japanese in: a name that doesn't fit, such as a reading's, is to be looked at.</summary>
   void WriteTheFieldsRecognizedAsTheJapanese()
   {
      _folder.Section("The fields recognized as the Japanese, by name");
      _folder.TableWithTheRestInAFile("japanese-fields",
                                      ["Field", "Note types", "Sentences"],
                                      [.. NoteTypes.Where(it => it.NoteType.Roles.JapaneseField is not null)
                                                   .GroupBy(it => it.NoteType.Roles.JapaneseField)
                                                   .OrderByDescending(it => it.Count())
                                                   .Select(it => new[] { it.Key ?? "", Invariant($"{it.Count():N0}"), Invariant($"{it.Sum(noteType => noteType.NoteType.SentenceCount):N0}") })]);
   }

   void WriteTheNoteTypesGivingNothing()
   {
      var givingNothing = NoteTypes.Where(it => it.NoteType.GivesNothingFromItsNotes).OrderByDescending(it => it.NoteType.NoteCount).ToList();
      _folder.Section("Note types with notes that give no sentence",
                      Invariant($"{givingNothing.Count:N0} note types, with {givingNothing.Sum(it => it.NoteType.NoteCount):N0} notes: no field was recognized as the Japanese, or the one that was holds none."));
      _folder.TableWithTheRestInAFile("note-types-giving-nothing", ["Deck", "Note type", "Notes", "Fields"],
                                      [.. givingNothing.Select(it => new[] { it.Deck.Deck.Name, it.NoteType.Roles.NoteType.Name, Invariant($"{it.NoteType.NoteCount:N0}"), string.Join(", ", it.NoteType.Roles.NoteType.FieldNames) })]);
   }

   void WriteTheNoteTypesGivingWords()
   {
      var givingWords = NoteTypes.Where(it => it.NoteType.GivesWordsRatherThanSentences).OrderByDescending(it => it.NoteType.SentenceCount).ToList();
      _folder.Section("Note types giving words rather than sentences",
                      Invariant($"{givingWords.Count:N0} note types, giving {givingWords.Sum(it => it.NoteType.SentenceCount):N0} \"sentences\" so short that they're most likely words, as a vocabulary deck's are."));
      _folder.TableWithTheRestInAFile("note-types-giving-words", ["Deck", "Note type", "Sentences", "Average length", "Japanese field", "Examples", "Fields"],
                                      [.. givingWords.Select(it => new[]
                                                                   {
                                                                      it.Deck.Deck.Name, it.NoteType.Roles.NoteType.Name, Invariant($"{it.NoteType.SentenceCount:N0}"), Invariant($"{it.NoteType.AverageSentenceLength:F1}"),
                                                                      it.NoteType.Roles.JapaneseField ?? "", string.Join(" / ", it.NoteType.ExampleSentences), string.Join(", ", it.NoteType.Roles.NoteType.FieldNames)
                                                                   })]);
   }

   void WriteEveryNoteType()
   {
      _folder.Section("Every note type", "In `note-types.tsv`: each deck's note types, with the fields recognized in them, what they give, and examples of their sentences.");
      _folder.TabSeparatedFile("note-types",
                               ["Deck", "Note type", "Notes", "Sentences", "Average length", "Japanese field", "Translation field", "Audio field", "Image field", "Examples", "Fields"],
                               NoteTypes.Select(it => new[]
                                                      {
                                                         it.Deck.Deck.Name, it.NoteType.Roles.NoteType.Name, Invariant($"{it.NoteType.NoteCount}"), Invariant($"{it.NoteType.SentenceCount}"),
                                                         Invariant($"{it.NoteType.AverageSentenceLength:F1}"), it.NoteType.Roles.JapaneseField ?? "", it.NoteType.Roles.TranslationField ?? "",
                                                         it.NoteType.Roles.AudioField ?? "", it.NoteType.Roles.ImageField ?? "", string.Join(" / ", it.NoteType.ExampleSentences),
                                                         string.Join(", ", it.NoteType.Roles.NoteType.FieldNames)
                                                      }));
   }
}
