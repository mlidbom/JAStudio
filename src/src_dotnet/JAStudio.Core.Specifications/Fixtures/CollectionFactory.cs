using System;
using System.Collections.Concurrent;
using Compze.Contracts;
using Compze.DependencyInjection;
using Compze.DependencyInjection.Abstractions;
using Compze.DependencyInjection.Microsoft.Extensions.Hosting;
using JAStudio.Core.Specifications.LanguageServices.ParsingCases;
using JAStudio.Core.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JAStudio.Core.Specifications.Fixtures;

[Flags]
public enum DataNeeded
{
   None = 0,
   Kanji = 2,
   /// <summary>The vocab of the parsing cases' corpus (<see cref="ParsingCasesFolder.Corpus"/>): real vocab of the corpus,<br/>
   /// committed with the specifications.</summary>
   Vocabulary = 4,
   Sentences = 8,
   All = Kanji | Vocabulary | Sentences
}

/// <summary>
/// Starts the specifications' JAStudios, each with a collection of its own, holding the notes <see cref="DataNeeded"/> says: loaded from the files of<br/>
/// <see cref="TheSpecificationsCorpus"/>, made once for each <see cref="DataNeeded"/>, as JAStudio loads the corpus when it starts.
/// </summary>
public static class CollectionFactory
{
   static readonly ConcurrentDictionary<DataNeeded, Lazy<TheSpecificationsCorpus>> Corpora = new();

   public static AppScope InjectCollectionWithSelectData(DataNeeded data, NotesKept notesKept = NotesKept.InMemory)
   {
      Contract.Argument.Assert(data == DataNeeded.None || notesKept == NotesKept.InMemory, () => "The specifications' corpus is loaded only into notes kept in memory.");
      var jastudio = StartAnApp(notesKept);
      if(data != DataNeeded.None) TheCorpusWith(data).LoadInto(jastudio.CoreApp);
      return jastudio;
   }

   /// <summary>A JAStudio of the specifications, its notes kept as <paramref name="notesKept"/> says, with no notes.</summary>
   internal static AppScope StartAnApp(NotesKept notesKept)
   {
      var plan = AppBootstrapper.PrepareForTests(notesKept);
      var host = Host.CreateDefaultBuilder()
                     .UseServiceProviderFactory(new MicrosoftServiceProviderFactory(plan.Builder))
                     .Build();
      host.StartAsync().GetAwaiter().GetResult();
      return new AppScope(host, host.Services.GetRequiredService<IRootResolver>().Resolve<CoreApp>());
   }

   static TheSpecificationsCorpus TheCorpusWith(DataNeeded data) =>
      Corpora.GetOrAdd(data, it => new Lazy<TheSpecificationsCorpus>(() => TheSpecificationsCorpus.With(it))).Value;

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
