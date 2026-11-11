using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Reservoir.Sources.AnkiPackages;

/// <summary>What a deck would give the reservoir, note type by note type (<see cref="WhatANoteTypeGives"/>), read without filling anything.</summary>
public sealed class WhatADeckGives
{
   WhatADeckGives(DeckOnDisk deck, IReadOnlyList<WhatANoteTypeGives> noteTypes)
   {
      Deck = deck;
      NoteTypes = noteTypes;
   }

   /// <summary>What <paramref name="deck"/> gives, its fields recognized as filling the reservoir recognizes them (<see cref="AnkiDeck"/>).</summary>
   public static WhatADeckGives Of(DeckOnDisk deck)
   {
      using var database = deck.OpenDatabase();
      return new WhatADeckGives(deck, [.. new AnkiDeck(database).FieldRoles.Select(roles => WhatANoteTypeGives.Of(deck, roles, database))]);
   }

   public DeckOnDisk Deck { get; }

   /// <summary>What each of the deck's note types that has notes gives.</summary>
   public IReadOnlyList<WhatANoteTypeGives> NoteTypes { get; }

   /// <summary>How many sentences the deck gives the reservoir, from all its note types.</summary>
   public int SentenceCount => NoteTypes.Sum(it => it.SentenceCount);
}
