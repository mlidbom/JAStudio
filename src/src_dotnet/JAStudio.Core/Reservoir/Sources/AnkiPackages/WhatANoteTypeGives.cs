using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What a note type of a deck would give the reservoir: the roles recognized in its fields (<see cref="DeckFieldRoles"/>), how many notes it has,<br/>
/// how many sentences they hold, how long those are, and a few of them, spread over its notes.
/// </summary>
/// <remarks>
/// Read before filling the reservoir from decks nobody has looked at, such as the 1,382 of the MediaFire folder, to see that the right fields were<br/>
/// recognized: a note type giving nothing though it has notes, or giving words where sentences were expected, is to be looked at.
/// </remarks>
public sealed class WhatANoteTypeGives
{
   const int ExamplesShown = 3;

   /// <summary>
   /// Sentences averaging fewer characters than this are most likely words, as a vocabulary deck's are, not sentences: a subs2srs deck's<br/>
   /// sentences average 10 to 20 characters.
   /// </summary>
   const double ShortestAverageLengthOfSentences = 6;

   WhatANoteTypeGives(DeckFieldRoles roles, int noteCount, IReadOnlyList<ReservoirSentenceText> sentences)
   {
      Roles = roles;
      NoteCount = noteCount;
      SentenceCount = sentences.Count;
      AverageSentenceLength = sentences.Count == 0 ? 0 : sentences.Average(it => it.Text.Length);
      ExampleSentences = [.. Enumerable.Range(0, ExamplesShown).Select(index => index * sentences.Count / ExamplesShown).Distinct().Where(index => index < sentences.Count).Select(index => sentences[index])];
   }

   /// <summary>What the notes of the note type <paramref name="roles"/> recognized the fields of give, read from <paramref name="database"/>.</summary>
   public static WhatANoteTypeGives Of(DeckFieldRoles roles, AnkiPackageDatabase database)
   {
      var notes = database.NotesOf(roles.NoteType).ToList();
      return new WhatANoteTypeGives(roles, notes.Count, [.. notes.Select(roles.SentenceOf).OfType<SourceSentence>().Select(it => it.Text)]);
   }

   public DeckFieldRoles Roles { get; }

   public int NoteCount { get; }

   /// <summary>How many of the notes hold a sentence in the field recognized as the Japanese.</summary>
   public int SentenceCount { get; }

   /// <summary>The average length of the sentences, in characters, cleaned as the reservoir keeps them.</summary>
   public double AverageSentenceLength { get; }

   /// <summary>A few of the sentences, spread evenly over the notes: the first, and others further on.</summary>
   public IReadOnlyList<ReservoirSentenceText> ExampleSentences { get; }

   /// <summary>Whether the note type has notes but gives no sentences: no field was recognized as the Japanese, or the one that was holds none.</summary>
   public bool GivesNothingFromItsNotes => NoteCount > 0 && SentenceCount == 0;

   /// <summary>Whether what the note type gives is most likely words, not sentences (<see cref="ShortestAverageLengthOfSentences"/>).</summary>
   public bool GivesWordsRatherThanSentences => SentenceCount > 0 && AverageSentenceLength < ShortestAverageLengthOfSentences;
}
