using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Threading;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.TaskRunners;

namespace JAStudio.Core.TestUtils;

/// <summary>
/// The student's Anki in specifications, where there's no Anki: it gives each note created a pretend study-note, and loading the collection<br/>
/// reads from it as from Anki (<see cref="IBackendDataLoader"/>): each study-note's corpus note, and the tags specifications gave it. Its cards have<br/>
/// no studying status.
/// </summary>
public class PretendAnki : IBackendNoteCreator, IBackendDataLoader
{
   readonly IMonitor _monitor = IMonitor.New();
   readonly Dictionary<long, NoteId> _noteOfEachStudyNote = [];
   readonly Dictionary<long, IReadOnlyList<string>> _tagsOfEachStudyNote = [];
   long _lastAnkiNoteId;

   public void CreateKanji(KanjiNote note, Action callback) => CreateTheStudyNoteOf(note, callback);
   public void CreateVocab(VocabNote note, Action callback) => CreateTheStudyNoteOf(note, callback);
   public void CreateSentence(SentenceNote note, Action callback) => CreateTheStudyNoteOf(note, callback);

   public BackendData Load(TaskRunner taskRunner) => _monitor.Locked(() => new BackendData(new Dictionary<long, NoteId>(_noteOfEachStudyNote), [], new Dictionary<long, IReadOnlyList<string>>(_tagsOfEachStudyNote)));

   /// <summary>
   /// Gives <paramref name="note"/>'s study-note the tags <paramref name="tagsInAnki"/>, in place of those it had, as the student tagging it in Anki does.<br/>
   /// The note shows them as its Anki tags once the collection loads again.
   /// </summary>
   public void Tag(JPNote note, params string[] tagsInAnki) =>
      _monitor.Locked(() => _tagsOfEachStudyNote[_noteOfEachStudyNote.Single(it => it.Value == note.GetId()).Key] = tagsInAnki);

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
