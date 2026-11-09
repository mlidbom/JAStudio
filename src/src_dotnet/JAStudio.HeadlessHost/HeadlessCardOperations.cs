using System;
using JAStudio.Core.Note;

namespace JAStudio.HeadlessHost;

/// <summary>Refuses to change a note's cards: the headless JAStudio has no Anki to change them in.</summary>
class HeadlessCardOperations : ICardOperations
{
   public void SuspendAllCardsForNote(NoteId noteId) => throw NoAnki();
   public void UnsuspendAllCardsForNote(NoteId noteId) => throw NoAnki();
   public void DeleteNote(NoteId noteId) => throw NoAnki();

   static NotSupportedException NoAnki() => new("The headless JAStudio has no Anki to change a note's cards in.");
}
