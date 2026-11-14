using System;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Specifications.Fixtures.BaseData.SampleData;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;
using JAStudio.Core.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.Core.Specifications.Fixtures;

[Flags]
public enum DataNeeded
{
   None = 0,
   Kanji = 2,
   /// <summary>The vocab of the parsing cases' stripped-down corpus (<see cref="ParsingCasesFolder.StrippedDownCorpus"/>): real vocab of the corpus,<br/>
   /// committed with the specifications.</summary>
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
      var app = resolver.Resolve<CoreApp>();

      if(data == DataNeeded.None)
         return new AppScope(host, app);

      var noteServices = app.Services.NoteServices;

      if(data.HasFlag(DataNeeded.Kanji))
      {
         foreach(var kanjiSpec in KanjiSpec.TestKanjiList)
         {
            var kanji = KanjiNote.Create(noteServices, kanjiSpec.Question, kanjiSpec.Answer);
            KanjiReadingsNotation.AddTo(kanji.Readings.On, kanjiSpec.OnReadings);
            KanjiReadingsNotation.AddTo(kanji.Readings.Kun, kanjiSpec.KunReadings);
         }
      }

      if(data.HasFlag(DataNeeded.Vocabulary))
      {
         ParsingCasesFolder.StrippedDownCorpus.AddTheVocabTo(app);
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

   public sealed class AppScope(IHost host, CoreApp coreApp) : IDisposable
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
