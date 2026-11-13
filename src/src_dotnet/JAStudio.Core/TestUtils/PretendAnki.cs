using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Tagging;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The student's Anki in specifications, where there's no Anki: it gives the notes specifications choose a pretend study-note<br/>
/// (<see cref="GiveAStudyNoteTo"/>), and loading the collection<br/>
/// reads from it as from Anki (<see cref="IBackendDataLoader"/>): each study-note's corpus note, the tags specifications or JAStudio gave it, and the<br/>
/// cards specifications suspended. It has no other cards, and no Anki notes but the study-notes. JAStudio changes its tags as it changes Anki's<br/>
/// (<see cref="IAnkiTagOperations"/>).
/// </summary>
public class PretendAnki : IBackendDataLoader, IAnkiTagOperations
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<long, NoteId> _noteOfEachStudyNote = [];
   readonly Dictionary<long, IReadOnlyList<string>> _tagsOfEachStudyNote = [];
   readonly Dictionary<long, List<CardStudyingStatus>> _suspendedCardsOfEachStudyNote = [];
   long _lastAnkiNoteId;

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

   /// <summary>The tags <paramref name="note"/>'s study-note has, as Anki names them, such as <c>marked</c>, in alphabetical order.</summary>
   public IReadOnlyList<string> TagsOf(JPNote note) => _monitor.Locked(() => _tagsOfEachStudyNote.GetValueOrDefault(AnkiNoteIdOf(note), []).Order(StringComparer.OrdinalIgnoreCase).ToList());

   public void AddTheTagToTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) =>
      ChangeTheTagsOf(notes.Select(AnkiNoteIdOf), tags => tags.Contains(tagInAnki, StringComparer.OrdinalIgnoreCase) ? tags : [..tags, tagInAnki]);

   public void RemoveTheTagFromTheStudyNotesOf(IReadOnlyCollection<NoteId> notes, string tagInAnki) =>
      ChangeTheTagsOf(notes.Select(AnkiNoteIdOf), tags => [..tags.Where(it => !it.Equals(tagInAnki, StringComparison.OrdinalIgnoreCase))]);

   public void RenameTheBranchOnEveryAnkiNote(string pathInAnki, string renamedPathInAnki)
   {
      var branch = new TagBranch(pathInAnki);
      var renamed = new TagBranch(renamedPathInAnki);
      ChangeTheTagsOf(EveryStudyNote(), tags => [..tags.Select(it => branch.Contains(it) ? branch.TheNameRenamedTo(renamed, it) : it).Distinct(StringComparer.OrdinalIgnoreCase)]);
   }

   public void DeleteTheBranchFromEveryAnkiNote(string pathInAnki) => ChangeTheTagsOf(EveryStudyNote(), tags => [..tags.Where(it => !new TagBranch(pathInAnki).Contains(it))]);

   public int NumberOfAnkiNotesWithATagIn(string pathInAnki) => _monitor.Locked(() => _tagsOfEachStudyNote.Values.Count(tags => tags.Any(new TagBranch(pathInAnki).Contains)));

   void ChangeTheTagsOf(IEnumerable<long> ankiNoteIds, Func<IReadOnlyList<string>, IReadOnlyList<string>> change) => _monitor.Locked(() =>
   {
      foreach(var ankiNoteId in ankiNoteIds.ToList())
         _tagsOfEachStudyNote[ankiNoteId] = change(_tagsOfEachStudyNote.TryGetValue(ankiNoteId, out var tags) ? tags : []);
   });

   List<long> EveryStudyNote() => _monitor.Locked(() => _noteOfEachStudyNote.Keys.ToList());

   long AnkiNoteIdOf(JPNote note) => AnkiNoteIdOf(note.GetId());

   long AnkiNoteIdOf(NoteId note) => _noteOfEachStudyNote.Single(it => it.Value.Equals(note)).Key;

   static string NoteTypeNameOf(JPNote note) => note switch
   {
      VocabNote => NoteTypes.Vocab,
      KanjiNote => NoteTypes.Kanji,
      SentenceNote => NoteTypes.Sentence,
      _ => throw new ArgumentOutOfRangeException(nameof(note), note, "Not a note type with a study-note.")
   };

   /// <summary>
   /// Gives <paramref name="note"/>, a note in the collection, a study-note in Anki, as a person choosing to study it will: registers a new Anki note id<br/>
   /// for it, so that what Anki reports about the study-note, such as its deletion or its cards' studying status, reaches the note. Creating a note<br/>
   /// gives it none (Magnus, 2026-11-13).
   /// </summary>
   public void GiveAStudyNoteTo(JPNote note)
   {
      var ankiNoteId = _monitor.Locked(() =>
      {
         _noteOfEachStudyNote[++_lastAnkiNoteId] = note.GetId();
         return _lastAnkiNoteId;
      });
      note.Services.ExternalNoteIdMap.Register(ankiNoteId, note.GetId());
   }
}
