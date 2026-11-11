using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Importing.RawNotes;

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

   WhatANoteTypeGives(DeckOnDisk deck, DeckFieldRoles roles, int noteCount, IReadOnlyList<ReservoirSentenceText> texts)
   {
      Deck = deck;
      Roles = roles;
      NoteCount = noteCount;
      TextCount = texts.Count;
      AverageTextLength = ReservoirSentenceText.AverageLengthOf(texts);
      Examples = ExamplesSpreadEvenlyOver(texts);
   }

   /// <summary>
   /// What the notes of the note type of <paramref name="deck"/> that <paramref name="roles"/> recognized the fields of give, read from<br/>
   /// <paramref name="database"/>, the deck's.
   /// </summary>
   public static WhatANoteTypeGives Of(DeckOnDisk deck, DeckFieldRoles roles, AnkiPackageDatabase database)
   {
      var notes = database.NotesOf(roles.NoteType).ToList();
      return new WhatANoteTypeGives(deck, roles, notes.Count, [.. notes.Select(roles.JapaneseTextOf).OfType<ReservoirSentenceText>()]);
   }

   /// <summary>The deck the note type is of.</summary>
   public DeckOnDisk Deck { get; }

   /// <summary>The roles recognized in the note type's fields.</summary>
   public DeckFieldRoles Roles { get; }

   public AnkiNoteType NoteType => Roles.NoteType;

   /// <summary>How many notes the deck has of the note type.</summary>
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

   /// <summary>Whether the field recognized as the Japanese holds words, not sentences (<see cref="DeckFieldRoles.JapaneseFieldHoldsWordsRatherThanSentences"/>).</summary>
   public bool GivesWordsRatherThanSentences => Roles.JapaneseFieldHoldsWordsRatherThanSentences;

   /// <summary>Up to <see cref="ExamplesShown"/> of <paramref name="texts"/>, spread evenly over them: the first, and others further on, each once.</summary>
   static IReadOnlyList<ReservoirSentenceText> ExamplesSpreadEvenlyOver(IReadOnlyList<ReservoirSentenceText> texts) =>
      [.. Enumerable.Range(0, ExamplesShown).Select(index => index * texts.Count / ExamplesShown).Distinct().Where(index => index < texts.Count).Select(index => texts[index])];
}
