using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Specifications.Fixtures;
using JAStudio.Core.TestUtils;

namespace JAStudio.Core.Specifications;

public abstract class SpecificationUsingACollection(DataNeeded data = DataNeeded.All) : IDisposable
{
   public virtual void Dispose() => _appScope.Dispose();

   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(data);

   protected TService GetService<TService>() where TService : class => _appScope.CoreApp.Services.Resolve<TService>();

   protected NoteServices NoteServices => GetService<NoteServices>();

   protected VocabNote CreateVocab(string question, string answer, params string[] readings) =>
      VocabNote.Create(NoteServices, question, answer, readings);

   protected VocabNote CreateVocab(string question, string answer, List<string> readings, List<string> forms) =>
      VocabNote.Create(NoteServices, question, answer, readings, forms);

   /// <summary>Creates a kanji with the readings <paramref name="onReadings"/> and <paramref name="kunReadings"/>, each written in the <see cref="KanjiReadingsNotation"/>.</summary>
   protected KanjiNote CreateKanji(string question, string answer, string onReadings, string kunReadings)
   {
      var kanji = KanjiNote.Create(NoteServices, question, answer);
      KanjiReadingsNotation.AddTo(kanji.Readings.On, onReadings);
      KanjiReadingsNotation.AddTo(kanji.Readings.Kun, kunReadings);
      return kanji;
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
