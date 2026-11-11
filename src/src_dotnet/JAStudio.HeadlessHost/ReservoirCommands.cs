using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Compze.DependencyInjection;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Feedback;
using JAStudio.Core.Reservoir.Keeping;
using JAStudio.Core.Reservoir.Measures;
using JAStudio.Core.Reservoir.Picking;
using JAStudio.Core.Reservoir.Reporting;
using JAStudio.Core.Reservoir.Sources.AnkiPackages;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's batch commands for the reservoir (<see cref="ReservoirFile"/>), run as <c>JAStudio.HeadlessHost reservoir &lt;command&gt;</c><br/>
/// followed by <c>--name value</c> pairs. Each runs to its end and exits, reporting as it goes on standard output.
/// </summary>
static class ReservoirCommands
{
   public const string CommandName = "reservoir";

   const string Usage = """
                        reservoir keep-decks --decks <an .apkg file, or a folder of them> --text-folder <folder> --media-folder <folder>
                        reservoir check-decks --folder <folder of .apkg files and of decks' collection databases> --report-folder <folder>
                        reservoir fill-decks --reservoir-file <file> --collection <name> --folder <folder of .apkg files and of decks' collection databases>
                        reservoir fill-subtitles --reservoir-file <file> --collection <name> --zip-or-folder <a zip or folder of a folder per show>
                        reservoir analyze --reservoir-file <file> --addon-folder <folder> --corpus-folder <folder> --anki-collection-file <file>
                        reservoir vocab --reservoir-file <file> --vocab <questions, separated by commas> --most-sentences <count> --addon-folder <folder> --corpus-folder <folder> --anki-collection-file <file>
                        reservoir report --reservoir-file <file> --report-folder <folder> --sentences-to-pick <count> --addon-folder <folder> --corpus-folder <folder> --anki-collection-file <file>
                        """;

   public static int Run(string[] args) => args switch
   {
      ["keep-decks", .. var values] => KeepDecks(CommandLineValues.From(values)),
      ["check-decks", .. var values] => CheckDecks(CommandLineValues.From(values)),
      ["fill-decks", .. var values] => FillDecks(CommandLineValues.From(values)),
      ["fill-subtitles", .. var values] => FillSubtitles(CommandLineValues.From(values)),
      ["analyze", .. var values] => Analyze(CommandLineValues.From(values)),
      ["vocab", .. var values] => TellWhatEachVocabDoes(CommandLineValues.From(values)),
      ["report", .. var values] => Report(CommandLineValues.From(values)),
      _ => throw new ArgumentException($"Unknown reservoir command: {string.Join(' ', args)}{Environment.NewLine}{Usage}")
   };

   /// <summary>
   /// Writes the report of what each deck in <c>--folder</c> would give the reservoir (<see cref="DeckCheckReport"/>), filling nothing: to check<br/>
   /// the fields recognized in decks nobody has looked at before filling the reservoir from them.
   /// </summary>
   /// <summary>
   /// Keeps the decks given as <c>--decks</c>, an .apkg file or a folder of them, as JAStudio's own (<see cref="ReservoirKeeping"/>): their text in<br/>
   /// <c>--text-folder</c>, meant for git, and their media in <c>--media-folder</c>, by content (<see cref="MediaStore"/>).
   /// </summary>
   static int KeepDecks(CommandLineValues values)
   {
      var keeping = new ReservoirKeeping(new DirectoryInfo(values.Value("text-folder")), new MediaStore(new DirectoryInfo(values.Value("media-folder"))));
      FileInfo[] decks = values.ExistingFileOrFolder("decks") switch
      {
         FileInfo package => [package],
         DirectoryInfo folder => folder.GetFiles("*.apkg", SearchOption.AllDirectories),
         var neither => throw new ArgumentException($"--decks: {neither.FullName} is neither a file nor a folder")
      };
      foreach(var deck in decks.OrderBy(it => it.Name, StringComparer.Ordinal)) Console.WriteLine(keeping.Keep(deck));
      return 0;
   }

   static int CheckDecks(CommandLineValues values)
   {
      var folder = values.ExistingFolder("folder");
      var decks = DeckOnDisk.EveryDeckIn(folder).AsParallel().Select(TheDeckChecked).OrderBy(it => it.Deck.Name, StringComparer.Ordinal).ToList();
      DeckCheckReport.WriteTo(new DirectoryInfo(values.Value("report-folder")), folder, decks);
      Console.WriteLine($"{decks.Count} decks checked, giving {decks.Sum(it => it.SentenceCount)} sentences.");
      return 0;

      static WhatADeckGives TheDeckChecked(DeckOnDisk deck)
      {
         var whatItGives = WhatADeckGives.Of(deck);
         Console.WriteLine($"{deck.Name}: {whatItGives.SentenceCount} sentences");
         return whatItGives;
      }
   }

   static int FillDecks(CommandLineValues values)
   {
      using var reservoir = ReservoirToFill(values);
      new ReservoirFilling(reservoir, Console.WriteLine).AddDecksIn(values.Value("collection"), values.ExistingFolder("folder"));
      TellTheReservoirsSize(reservoir);
      return 0;
   }

   static int FillSubtitles(CommandLineValues values)
   {
      using var reservoir = ReservoirToFill(values);
      new ReservoirFilling(reservoir, Console.WriteLine).AddSubtitlesIn(values.Value("collection"), values.ExistingFileOrFolder("zip-or-folder"));
      TellTheReservoirsSize(reservoir);
      return 0;
   }

   /// <summary>Analyzes every sentence with no analysis yet, with the vocab of the corpus checkout the headless JAStudio loads (<see cref="HeadlessJAStudioSetup"/>).</summary>
   static int Analyze(CommandLineValues values)
   {
      using var reservoir = ExistingReservoir(values);
      using var jastudio = LoadedHeadlessJAStudio.From(HeadlessJAStudioSetup.From(values));
      new ReservoirAnalyzing(reservoir, jastudio.Root.Resolve<AnalysisServices>(), Console.WriteLine).AnalyzeEveryUnanalyzedSentence();
      return 0;
   }

   /// <summary>Tells what each vocab named by <c>--vocab</c> does in the reservoir, as the corpus checkout has it now (<see cref="WhatAVocabDoesInTheReservoir"/>).</summary>
   static int TellWhatEachVocabDoes(CommandLineValues values)
   {
      using var reservoir = ExistingReservoir(values);
      using var jastudio = LoadedHeadlessJAStudio.From(HeadlessJAStudioSetup.From(values));
      var texts = ReservoirSentenceTexts.LoadedFrom(reservoir);
      Console.WriteLine($"{texts.Count:N0} sentence texts loaded.");
      var services = jastudio.Root.Resolve<AnalysisServices>();
      var mostSentences = values.Integer("most-sentences");
      foreach(var vocab in values.Value("vocab").Split(',').SelectMany(question => services.Vocab.WithQuestion(question.Trim())))
      {
         var watch = Stopwatch.StartNew();
         var whatItDoes = WhatAVocabDoesInTheReservoir.Of(vocab, reservoir, texts, services, mostSentences);
         Console.WriteLine($"{whatItDoes.Described()}({watch.Elapsed.TotalSeconds:F1} s){Environment.NewLine}");
      }

      return 0;
   }

   /// <summary>
   /// Writes the report of what the analyzed sentences could give the corpus of the corpus checkout the headless JAStudio loads<br/>
   /// (<see cref="ReservoirReport"/>), picking up to <c>--sentences-to-pick</c> sentences worth taking in.
   /// </summary>
   static int Report(CommandLineValues values)
   {
      using var reservoir = ExistingReservoir(values);
      using var jastudio = LoadedHeadlessJAStudio.From(HeadlessJAStudioSetup.From(values));
      var collection = jastudio.Root.Resolve<JPCollection>();
      Console.WriteLine("Counting what the corpus has...");
      var corpus = WhatTheCorpusHas.In(collection);
      Console.WriteLine("Measuring the reservoir...");
      var measures = ReservoirMeasures.Of(reservoir);
      Console.WriteLine("Picking the sentences worth taking in...");
      var picks = SentencesWorthTakingIn.PickedFrom(reservoir, measures, corpus, values.Integer("sentences-to-pick"));
      Console.WriteLine("Writing the report...");
      ReservoirReport.WriteTo(new DirectoryInfo(values.Value("report-folder")), reservoir, measures, corpus, collection, picks);
      return 0;
   }

   /// <summary>The reservoir given as <c>--reservoir-file</c>, created empty when there's no such file yet.</summary>
   static ReservoirFile ReservoirToFill(CommandLineValues values) => ReservoirFile.OpenOrCreate(new FileInfo(values.Value("reservoir-file")));

   /// <summary>The reservoir given as <c>--reservoir-file</c>, which must exist.</summary>
   static ReservoirFile ExistingReservoir(CommandLineValues values) => ReservoirFile.OpenOrCreate(values.ExistingFile("reservoir-file"));

   static void TellTheReservoirsSize(ReservoirFile reservoir) => Console.WriteLine($"The reservoir holds {reservoir.SentenceCount} sentences.");
}
