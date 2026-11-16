using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Studying;

/// <summary>
/// What the SRS, the spaced repetition system, holds of each note's study-note that JAStudio reads and never stores, by the note's id: the studying<br/>
/// status of each of the study-note's cards, and the tags the student gave it (<see cref="AnkiTags"/>). Read from the spaced repetition system as<br/>
/// the collection loads, and again when it reads the study-notes again (<see cref="JPCollection.ReadTheStudyNotesFromTheSRSAgain"/>). A note<br/>
/// answers from it, as in <see cref="JPNote.IsStudying"/> and <see cref="JPNote.AnkiTags"/>.
/// </summary>
/// <remarks>
/// It isn't the notes' own data, so the notes don't hold it: it changes without a note being edited, a read from the spaced repetition system can<br/>
/// change it for every note at once, and a note built again from its stored data, as a unit of work's copy of it is, doesn't have it.
/// </remarks>
/// <remarks>
/// Any number of threads read and change it at once. What is held of one study-note is replaced whole, so a reader sees it either as it was or as<br/>
/// it is after the change.<br/>
/// review-naming:studying: a name of Claude's, 2026-11-16, for what <see cref="JPNote"/> held itself until then, as its card statuses and its<br/>
/// <see cref="JPNote.AnkiTags"/>.
/// </remarks>
partial class WhatTheSRSHoldsOfTheStudyNotes
{
   readonly ConcurrentDictionary<NoteId, OfOneStudyNote> _ofTheStudyNoteOfEachNote = new();

   /// <summary>
   /// Takes in what the spaced repetition system holds of the study-note of the note with the id <paramref name="noteId"/>, in place of what was<br/>
   /// held: the studying statuses of its cards, <paramref name="cardStatuses"/>, and its tags there, <paramref name="tagsInTheSRS"/>. Whether that<br/>
   /// differs from what was held.
   /// </summary>
   public bool TakeIn(NoteId noteId, IEnumerable<CardStudyingStatus> cardStatuses, IEnumerable<string> tagsInTheSRS)
   {
      var heldBefore = Of(noteId);
      var takenIn = new OfOneStudyNote(cardStatuses.ToDictionary(it => it.CardType, it => !it.IsSuspended), new AnkiTags(tagsInTheSRS));
      Hold(noteId, takenIn);
      return !takenIn.IsTheSameAs(heldBefore);
   }

   /// <summary>Forgets what was held of the study-note of the note with the id <paramref name="noteId"/>: as when it's gone from the spaced repetition system.</summary>
   public void Forget(NoteId noteId) => _ofTheStudyNoteOfEachNote.TryRemove(noteId, out _);

   /// <summary>
   /// Holds <paramref name="status"/> as the studying status of the card it is of, of the study-note of the note with the id <paramref name="noteId"/>,<br/>
   /// in place of what was held of that card.
   /// </summary>
   public void SetStudyingStatus(NoteId noteId, CardStudyingStatus status) =>
      _ofTheStudyNoteOfEachNote.AddOrUpdate(noteId,
                                            _ => OfOneStudyNote.None.WithTheCard(status.CardType, isActive: !status.IsSuspended),
                                            (_, held) => held.WithTheCard(status.CardType, isActive: !status.IsSuspended));

   /// <summary>
   /// Marks every card held of the study-note of the note with the id <paramref name="noteId"/> as active, that is unsuspended, or as suspended:<br/>
   /// after the spaced repetition system was told to do the same.
   /// </summary>
   public void SetTheStatusOfEveryKnownCard(NoteId noteId, bool isActive)
   {
      if(_ofTheStudyNoteOfEachNote.TryGetValue(noteId, out var held))
         _ofTheStudyNoteOfEachNote[noteId] = held.WithEveryCard(isActive);
   }

   /// <summary>
   /// Whether the note with the id <paramref name="noteId"/> is studied: through the card of <paramref name="cardType"/>, such as<br/>
   /// <see cref="CardTypes.Reading"/>, or through any of its cards when it's null. A card that isn't suspended is studied.
   /// </summary>
   public bool IsStudying(NoteId noteId, string? cardType) => Of(noteId).IsStudying(cardType);

   /// <summary>Whether any card of the study-note of the note with the id <paramref name="noteId"/> is suspended.</summary>
   public bool HasSuspendedCards(NoteId noteId) => Of(noteId).HasSuspendedCards;

   /// <summary>The tags the study-note of the note with the id <paramref name="noteId"/> has in Anki: none when it has no study-note.</summary>
   public AnkiTags AnkiTagsOf(NoteId noteId) => Of(noteId).AnkiTags;

   OfOneStudyNote Of(NoteId noteId) => _ofTheStudyNoteOfEachNote.GetValueOrDefault(noteId, OfOneStudyNote.None);

   void Hold(NoteId noteId, OfOneStudyNote ofTheStudyNote)
   {
      if(ofTheStudyNote.IsNothing)
         Forget(noteId);
      else
         _ofTheStudyNoteOfEachNote[noteId] = ofTheStudyNote;
   }
}
