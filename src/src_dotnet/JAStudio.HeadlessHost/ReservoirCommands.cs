using System;
using System.IO;
using Compze.DependencyInjection;
using JAStudio.Core.LanguageServices.JanomeEx;
using JAStudio.Core.Reservoir;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Reservoir.Analysis;
using JAStudio.Core.Reservoir.Measures;

namespace JAStudio.HeadlessHost;

/// <summary>
/// The headless JAStudio's batch commands for the reservoir (<see cref="ReservoirFile"/>), run as <c>JAStudio.HeadlessHost reservoir &lt;command&gt;</c><br/>
/// followed by <c>--name value</c> pairs. Each runs to its end and exits, reporting as it goes on standard output.
/// </summary>
static class ReservoirCommands
{
   public const string CommandName = "reservoir";

   const string Usage = """
                        reservoir fill-decks --reservoir-file <file> --collection <name> --folder <folder of .apkg files and of decks' collection databases>
                        reservoir fill-subtitles --reservoir-file <file> --collection <name> --zip-or-folder <a zip or folder of a folder per show>
                        reservoir analyze --reservoir-file <file> --addon-folder <folder> --corpus-folder <folder> --anki-collection-file <file>
                        reservoir report --reservoir-file <file> --report-folder <folder> --sentences-to-pick <count> --addon-folder <folder> --corpus-folder <folder> --anki-collection-file <file>
                        """;

   public static int Run(string[] args) => args switch
   {
      ["fill-decks", .. var values] => FillDecks(CommandLineValues.From(values)),
      ["fill-subtitles", .. var values] => FillSubtitles(CommandLineValues.From(values)),
      ["analyze", .. var values] => Analyze(CommandLineValues.From(values)),
      ["report", .. var values] => Report(CommandLineValues.From(values)),
      _ => throw new ArgumentException($"Unknown reservoir command: {string.Join(' ', args)}{Environment.NewLine}{Usage}")
   };

   static int FillDecks(CommandLineValues values)
   {
      using var reservoir = ReservoirFile.OpenOrCreate(new FileInfo(values.Value("reservoir-file")));
      new ReservoirFilling(reservoir, Console.WriteLine).AddDecksIn(values.Value("collection"), values.ExistingFolder("folder"));
      Console.WriteLine($"The reservoir holds {reservoir.SentenceCount} sentences.");
      return 0;
   }

   /// <summary>Analyzes every sentence with no analysis yet, with the vocab of the corpus checkout the headless JAStudio loads (<see cref="HeadlessJAStudioSetup"/>).</summary>
   static int Analyze(CommandLineValues values)
   {
      using var reservoir = ReservoirFile.OpenOrCreate(values.ExistingFile("reservoir-file"));
      using var jastudio = LoadedHeadlessJAStudio.From(HeadlessJAStudioSetup.From(values));
      new ReservoirAnalyzing(reservoir, jastudio.Root.Resolve<AnalysisServices>(), Console.WriteLine).AnalyzeEveryUnanalyzedSentence();
      return 0;
   }

   /// <summary>
   /// Writes the report of what the analyzed sentences could give the corpus of the corpus checkout the headless JAStudio loads<br/>
   /// (<see cref="ReservoirReport"/>), picking up to <c>--sentences-to-pick</c> sentences worth taking in.
   /// </summary>
   static int Report(CommandLineValues values)
   {
      using var reservoir = ReservoirFile.OpenOrCreate(values.ExistingFile("reservoir-file"));
      using var jastudio = LoadedHeadlessJAStudio.From(HeadlessJAStudioSetup.From(values));
      var collection = jastudio.Root.Resolve<JPCollection>();
      Console.WriteLine("Counting what the corpus has...");
      var corpus = WhatTheCorpusHas.In(collection);
      Console.WriteLine("Measuring the reservoir...");
      var measures = ReservoirMeasures.Of(reservoir);
      Console.WriteLine("Picking the sentences worth taking in...");
      var picks = SentencesWorthTakingIn.PickedFrom(reservoir, measures, corpus, int.Parse(values.Value("sentences-to-pick"), System.Globalization.CultureInfo.InvariantCulture));
      Console.WriteLine("Writing the report...");
      ReservoirReport.WriteTo(new DirectoryInfo(values.Value("report-folder")), reservoir, measures, corpus, collection, picks);
      return 0;
   }

   static int FillSubtitles(CommandLineValues values)
   {
      using var reservoir = ReservoirFile.OpenOrCreate(new FileInfo(values.Value("reservoir-file")));
      var zipOrFolder = values.Value("zip-or-folder");
      new ReservoirFilling(reservoir, Console.WriteLine).AddSubtitlesIn(values.Value("collection"), File.Exists(zipOrFolder) ? new FileInfo(zipOrFolder) : values.ExistingFolder("zip-or-folder"));
      Console.WriteLine($"The reservoir holds {reservoir.SentenceCount} sentences.");
      return 0;
   }
}
