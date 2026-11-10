using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The student's Anki in specifications, where there's no Anki: it gives each note created a pretend study-note, and loading the collection<br/>
/// reads from it as from Anki (<see cref="IBackendDataLoader"/>): each study-note's corpus note, the tags specifications gave it, and the cards they<br/>
/// suspended. It has no other cards.
/// </summary>
public class PretendAnki : IBackendNoteCreator, IBackendDataLoader
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<long, NoteId> _noteOfEachStudyNote = [];
   readonly Dictionary<long, IReadOnlyList<string>> _tagsOfEachStudyNote = [];
   readonly Dictionary<long, List<CardStudyingStatus>> _suspendedCardsOfEachStudyNote = [];
   long _lastAnkiNoteId;

   public void CreateKanji(KanjiNote note, Action callback) => CreateTheStudyNoteOf(note, callback);
   public void CreateVocab(VocabNote note, Action callback) => CreateTheStudyNoteOf(note, callback);
   public void CreateSentence(SentenceNote note, Action callback) => CreateTheStudyNoteOf(note, callback);

   public BackendData Load(TaskRunner taskRunner) => _monitor.Locked(() => new BackendData(new Dictionary<long, NoteId>(_noteOfEachStudyNote),
                                                                                          [.._suspendedCardsOfEachStudyNote.Values.SelectMany(it => it)],
                                                                                          new Dictionary<long, IReadOnlyList<string>>(_tagsOfEachStudyNote)));

   /// <summary>
   /// Gives <paramref name="note"/>'s study-note the tags <paramref name="tagsInAnki"/>, in place of those it had, as the student tagging it in Anki does.<br/>
   /// The note shows them as its Anki tags once the collection loads again.
   /// </summary>
   public void Tag(JPNote note, params string[] tagsInAnki) => _monitor.Locked(() => _tagsOfEachStudyNote[AnkiNoteIdOf(note)] = tagsInAnki);

   /// <summary>
   /// Suspends the card of <paramref name="cardType"/>, such as <see cref="CardTypes.Reading"/>, of <paramref name="note"/>'s study-note, as the student<br/>
   /// suspending it in Anki does. The note shows it once the collection loads, or reads the study-notes from Anki, again.
   /// </summary>
   public void SuspendTheCard(JPNote note, string cardType) => _monitor.Locked(() =>
   {
      var ankiNoteId = AnkiNoteIdOf(note);
      if(!_suspendedCardsOfEachStudyNote.TryGetValue(ankiNoteId, out var suspendedCards)) _suspendedCardsOfEachStudyNote[ankiNoteId] = suspendedCards = [];
      suspendedCards.Add(new CardStudyingStatus(ankiNoteId, cardType, isSuspended: true, NoteTypeNameOf(note)));
   });

   /// <summary>
   /// Deletes <paramref name="note"/>'s study-note, as the student deleting it in Anki on another device, and then syncing, does: nothing tells JAStudio.<br/>
   /// The note shows it once the collection loads, or reads the study-notes from Anki, again.
   /// </summary>
   public void DeleteTheStudyNoteOf(JPNote note) => _monitor.Locked(() =>
   {
      var ankiNoteId = AnkiNoteIdOf(note);
      _noteOfEachStudyNote.Remove(ankiNoteId);
      _tagsOfEachStudyNote.Remove(ankiNoteId);
      _suspendedCardsOfEachStudyNote.Remove(ankiNoteId);
   });

   long AnkiNoteIdOf(JPNote note) => _noteOfEachStudyNote.Single(it => it.Value.Equals(note.GetId())).Key;

   static string NoteTypeNameOf(JPNote note) => note switch
   {
      VocabNote => NoteTypes.Vocab,
      KanjiNote => NoteTypes.Kanji,
      SentenceNote => NoteTypes.Sentence,
      _ => throw new ArgumentOutOfRangeException(nameof(note), note, "Not a note type with a study-note.")
   };

   /// <summary>
   /// Registers a new Anki note id for <paramref name="note"/>, as creating its real Anki note does, so that what Anki reports about the Anki note,<br/>
   /// such as its deletion or its cards' studying status, reaches the note. Then, as in Anki, the note is saved where the corpus keeps notes.
   /// </summary>
   void CreateTheStudyNoteOf(JPNote note, Action callback)
   {
      var ankiNoteId = _monitor.Locked(() =>
      {
         _noteOfEachStudyNote[++_lastAnkiNoteId] = note.GetId();
         return _lastAnkiNoteId;
      });
      note.Services.ExternalNoteIdMap.Register(ankiNoteId, note.GetId());
      callback();
      note.UpdateInCache();
   }
}
