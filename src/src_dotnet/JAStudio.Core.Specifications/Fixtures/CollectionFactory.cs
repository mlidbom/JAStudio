using System;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.Core.Specifications.Fixtures;

public enum DataNeeded
{
   None = 0,
   Kanji = 2,
   Vocabulary = 4,
   Sentences = 8,
   All = Kanji | Vocabulary | Sentences
}

public static class CollectionFactory
{
   public static AppScope InjectCollectionWithSelectData(DataNeeded data)
   {
      var plan = AppBootstrapper.PrepareForTests();
      var host = Host.CreateDefaultBuilder()
                     .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                     .Build();
      host.StartAsync().GetAwaiter().GetResult();

      var resolver = host.Services.GetRequiredService<IRootResolver>();
      TemporaryServiceCollection.Instance = resolver.Resolve<TemporaryServiceCollection>();
      var app = resolver.Resolve<CoreApp>();

      if(data == DataNeeded.None)
         return new AppScope(host, app);

      var noteServices = app.Services.NoteServices;

      if(data.HasFlag(DataNeeded.Kanji))
      {
         foreach(var kanjiSpec in KanjiSpec.TestKanjiList)
         {
            KanjiNote.Create(noteServices, kanjiSpec.Question, kanjiSpec.Answer, kanjiSpec.OnReadings, kanjiSpec.KunReading);
         }
      }

      if(data.HasFlag(DataNeeded.Vocabulary))
      {
         foreach(var vocab in VocabLists.TestSpecialVocab)
         {
            app.Services.VocabNoteFactory.Create(vocab.DisambiguationName, vocab.Answer, vocab.Readings, vocab.InitializeNote);
         }
      }

      if(data.HasFlag(DataNeeded.Sentences))
      {
         foreach(var sentence in SentenceSpec.TestSentenceList)
         {
            SentenceNote.CreateTestNote(noteServices, sentence.Question, sentence.Answer);
         }
      }

      return new AppScope(host, app);
   }

   public class AppScope(IHost host, CoreApp coreApp) : IDisposable
   {
      readonly IHost _host = host;
      public CoreApp CoreApp { get; } = coreApp;

      public void Dispose()
      {
         CoreApp.Dispose();
         _host.StopAsync().GetAwaiter().GetResult();
         _host.Dispose();
      }
   }
}
