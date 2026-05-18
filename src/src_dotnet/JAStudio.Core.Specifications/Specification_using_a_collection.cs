using System;
using System.Collections.Generic;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Specifications.Fixtures;

namespace JAStudio.Core.Specifications;

public abstract class SpecificationUsingACollection(DataNeeded data = DataNeeded.All) : IDisposable
{
   public void Dispose() => _appScope.Dispose();

   readonly CollectionFactory.AppScope _appScope = CollectionFactory.InjectCollectionWithSelectData(data);

   protected TService GetService<TService>() where TService : class => _appScope.CoreApp.Services.Resolve<TService>();

   protected NoteServices NoteServices => GetService<NoteServices>();

   protected VocabNote CreateVocab(string question, string answer, params string[] readings) =>
      VocabNote.Create(NoteServices, question, answer, readings);

   protected VocabNote CreateVocab(string question, string answer, List<string> readings, List<string> forms) =>
      VocabNote.Create(NoteServices, question, answer, readings, forms);

   protected KanjiNote CreateKanji(string question, string answer, string onReadings, string kunReading) =>
      KanjiNote.Create(NoteServices, question, answer, onReadings, kunReading);

   protected SentenceNote CreateSentence(string question) =>
      SentenceNote.Create(NoteServices, question);

   protected SentenceNote CreateTestSentence(string question, string answer) =>
      SentenceNote.CreateTestNote(NoteServices, question, answer);
}
