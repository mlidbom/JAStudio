using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>
/// What a note type of a deck would give the reservoir: the roles recognized in its fields (<see cref="DeckFieldRoles"/>), how many notes it has,<br/>
/// how many hold Japanese in the field recognized as the Japanese, how long that is, a few examples, spread over its notes, and so how many<br/>
/// sentences it gives.
/// </summary>
/// <remarks>
/// Read before filling the reservoir from decks nobody has looked at, such as the 1,382 of the MediaFire folder, to see that the right fields were<br/>
/// recognized: a note type giving nothing though it has notes, or holding words where sentences were expected, is to be looked at.
/// </remarks>
public sealed class WhatANoteTypeGives
{
   const int ExamplesShown = 3;

   WhatANoteTypeGives(DeckFieldRoles roles, int noteCount, IReadOnlyList<ReservoirSentenceText> texts)
   {
      Roles = roles;
      NoteCount = noteCount;
      TextCount = texts.Count;
      AverageTextLength = texts.Count == 0 ? 0 : texts.Average(it => it.Text.Length);
      Examples = [.. Enumerable.Range(0, ExamplesShown).Select(index => index * texts.Count / ExamplesShown).Distinct().Where(index => index < texts.Count).Select(index => texts[index])];
   }

   /// <summary>What the notes of the note type <paramref name="roles"/> recognized the fields of give, read from <paramref name="database"/>.</summary>
   public static WhatANoteTypeGives Of(DeckFieldRoles roles, AnkiPackageDatabase database)
   {
      var notes = database.NotesOf(roles.NoteType).ToList();
      return new WhatANoteTypeGives(roles, notes.Count, [.. notes.Select(roles.JapaneseTextOf).OfType<ReservoirSentenceText>()]);
   }

   public DeckFieldRoles Roles { get; }

   public int NoteCount { get; }

   /// <summary>How many of the notes hold Japanese in the field recognized as the Japanese: sentences, or words when it holds words.</summary>
   public int TextCount { get; }

   /// <summary>The average length of the Japanese, in characters, cleaned as the reservoir keeps it.</summary>
   public double AverageTextLength { get; }

   /// <summary>A few of the notes' Japanese, spread evenly over the notes: the first, and others further on.</summary>
   public IReadOnlyList<ReservoirSentenceText> Examples { get; }

   /// <summary>How many sentences the note type gives the reservoir: none when it holds words rather than sentences.</summary>
   public int SentenceCount => GivesWordsRatherThanSentences ? 0 : TextCount;

   /// <summary>Whether the note type has notes but gives no Japanese: no field was recognized as the Japanese, or the one that was holds none.</summary>
   public bool GivesNothingFromItsNotes => NoteCount > 0 && TextCount == 0;

   /// <summary>Whether the field recognized as the Japanese holds words, not sentences (<see cref="DeckFieldRoles.HoldsWordsRatherThanSentences"/>).</summary>
   public bool GivesWordsRatherThanSentences => Roles.HoldsWordsRatherThanSentences;
}
