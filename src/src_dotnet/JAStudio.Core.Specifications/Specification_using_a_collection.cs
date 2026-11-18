using System;
using System.Collections.Generic;
using System.Threading;
using Compze.Internals.SystemCE.ThreadingCE.TasksCE;
using Compze.Tessaging.Engine;
using Compze.Tessaging.TessageTypes;
using Compze.Teventive.MemoryImageStateStore;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.Storage;
using JAStudio.Core.TaskRunners;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core.Specifications;

public abstract class SpecificationUsingACollection(DataNeeded data = DataNeeded.All, NotesKept notesKept = NotesKept.InMemory) : IDisposable
{
   /// <summary>
   /// Disposes the app once the work the specification started in the background has finished, and every tevent published has been observed, and<br/>
   /// fails the specification if that work failed.
   /// </summary>
   public virtual void Dispose()
   {
      try
      {
         GetService<RethrowingFatalErrorHandler>().WaitUntilTheStartedWorkHasFinished();
         WaitUntilEveryTeventPublishedHasBeenObserved();
      }
      finally
      {
         _appScope.Dispose();
      }
   }

   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(data, notesKept);

   protected TService GetService<TService>() where TService : class => _appScope.CoreApp.Services.Resolve<TService>();

   protected NoteServices NoteServices => GetService<NoteServices>();

   /// <summary>The text of <paramref name="note"/>'s file, as the memory image state store last wrote it, or as <see cref="WriteTheFileOf"/><br/>
   /// wrote it.</summary>
   protected string TheStoredFileOf(JPNote note) => GetService<ITaggregateFilesInMemory>().ReadAllText(PathOfTheFileOf(note));

   /// <summary>
   /// Writes <paramref name="note"/>'s file as it is now, behind the memory image state store's back, as an edit to the corpus checkout on the<br/>
   /// disk would: what the<br/>
   /// next reload of the collection sees.
   /// </summary>
   protected void WriteTheFileOf(JPNote note) => GetService<ITaggregateFilesInMemory>().WriteAllText(PathOfTheFileOf(note), GetService<NoteSerializer>().Serialize(note));

   string PathOfTheFileOf(JPNote note) => GetService<CorpusNoteFiles>().FileOf(note).FullName;

   /// <summary>The tevents observed so far (<see cref="TeventsObserved"/>), once every tevent published so far has been.</summary>
   protected IReadOnlyList<ITevent> TeventsObservedSoFar()
   {
      WaitUntilEveryTeventPublishedHasBeenObserved();
      return GetService<TeventsObserved>().SoFar();
   }

   /// <summary>
   /// Waits until every tevent published so far has been observed, and the calls posted to the UI thread meanwhile have run on this thread, which is<br/>
   /// the UI thread (<see cref="NoUIThreadDispatcher"/>): what the application does once a change has committed, such as a view model showing it,<br/>
   /// has then been done. A posted call may publish tevents of its own, so it waits until neither is left.
   /// </summary>
   protected void WaitUntilEveryTeventPublishedHasBeenObserved()
   {
      do GetService<ILocalTessagingEngine>().AwaitNoTessagesInFlightAsync().WaitUnwrappingException();
      while(RunWhatWasPostedToTheUIThread());
   }

   /// <summary>
   /// Waits until <paramref name="condition"/> holds, for at most <paramref name="timeout"/>, running meanwhile the calls posted to the UI thread, which<br/>
   /// this thread is (<see cref="NoUIThreadDispatcher"/>), as a UI thread keeps doing while it waits: a view model shows a change through such a call.
   /// </summary>
   /// <returns>Whether <paramref name="condition"/> held before <paramref name="timeout"/> passed.</returns>
   protected bool KeepRunningTheUIThreadUntil(Func<bool> condition, TimeSpan timeout) => SpinWait.SpinUntil(() =>
   {
      RunWhatWasPostedToTheUIThread();
      return condition();
   }, timeout);

   /// <summary>Runs the calls posted to the UI thread so far, on this thread (<see cref="NoUIThreadDispatcher.RunWhatWasPosted"/>).</summary>
   /// <returns>Whether it ran any.</returns>
   bool RunWhatWasPostedToTheUIThread() => GetService<IUIThreadDispatcher>() is NoUIThreadDispatcher theUIThread && theUIThread.RunWhatWasPosted();

   protected VocabNote CreateVocab(string question, string answer, params string[] readings) =>
      VocabNote.Create(NoteServices, question, answer, readings);

   protected VocabNote CreateVocab(string question, string answer, List<string> readings, List<string> forms) =>
      VocabNote.Create(NoteServices, question, answer, readings, forms);

   /// <summary>Creates a kanji with the readings <paramref name="onReadings"/> and <paramref name="kunReadings"/>, each written in the <see cref="KanjiReadingsNotation"/>.</summary>
   protected KanjiNote CreateKanji(string question, string answer, string onReadings, string kunReadings)
   {
      var kanji = KanjiNote.Create(NoteServices, question, answer);
      KanjiReadingsNotation.AddTo(kanji, onReadings, kunReadings);
      return kanji.Reference().Current;
   }

   /// <summary>
   /// <paramref name="note"/>, given a study-note in the pretend Anki, as a person choosing to study it will (<see cref="PretendAnki.GiveAStudyNoteTo"/>):<br/>
   /// creating a note gives it none.
   /// </summary>
   protected TNote WithAStudyNote<TNote>(TNote note) where TNote : JPNote
   {
      GetService<PretendAnki>().GiveAStudyNoteTo(note);
      return note;
   }

   protected SentenceNote CreateSentence(string question) =>
      SentenceNote.Create(NoteServices, question);

   protected SentenceNote CreateTestSentence(string question, string answer) =>
      SentenceNote.CreateTestNote(NoteServices, question, answer);
}
