using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.Tokenizing;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.LanguageServices.Parsing.WordExtraction;

public sealed class TextAnalysis
{
   public const string Version = "text_analysis_0.1";

   static readonly MeCabTokenizer Tokenizer = MeCabTokenizer.GetInstance();

   public AnalysisServices Services { get; }

   /// <summary>
   /// The options of the sentence breakdown this analysis is made for. Null for an analysis made as a sentence's stored parsing result is, which<br/>
   /// no option changes.
   /// </summary>
   public BreakdownParsingOptions? BreakdownParsingOptions { get; }

   public string Text { get; }
   public SentenceConfiguration Configuration { get; }
   public MeCabTokenizedText TokenizedText { get; }
   public List<AnalysisToken> PreProcessedTokens { get; }
   public List<TextAnalysisLocation> Locations { get; }
   public TextAnalysisLocation StartLocation { get; }
   public List<CandidateWordVariant> IndexingWordVariants { get; }
   public List<CandidateWordVariant> DisplayWordVariants { get; }
   public List<Match> ValidMatches { get; }
   public List<Match> DisplayMatches { get; }

   /// <summary>
   /// Every match the analysis made, valid or not: those of every variant of every candidate word at every location. A rejected match keeps<br/>
   /// why, in its <see cref="Match.FailureReasons"/>.
   /// </summary>
   public IEnumerable<Match> EveryMatch => Locations.SelectMany(it => it.CandidateWords).SelectMany(it => it.Variants).SelectMany(it => it.Matches);

   /// <summary>The vocab matches the analysis rejected, each with why, in its <see cref="Match.FailureReasons"/>.</summary>
   public IEnumerable<VocabMatch> RejectedVocabMatches => EveryMatch.OfType<VocabMatch>().Where(it => !it.IsValid);

   /// <summary>Every match of <paramref name="vocab"/> in the sentence, valid or not: one for each word in it that is one of the vocab's forms.</summary>
   public IReadOnlyList<VocabMatch> MatchesOf(VocabNote vocab) => [.. EveryMatch.OfType<VocabMatch>().Where(it => it.Vocab.Equals(vocab))];

   /// <summary>
   /// Analyzes <paramref name="sentence"/>: for the sentence breakdown under <paramref name="breakdownParsingOptions"/>, or, without them, as a<br/>
   /// sentence's stored parsing result is made.
   /// </summary>
   public TextAnalysis(AnalysisServices services, string sentence, SentenceConfiguration sentenceConfiguration, BreakdownParsingOptions? breakdownParsingOptions = null)
   {
      Services = services;
      BreakdownParsingOptions = breakdownParsingOptions;
      Text = sentence;
      Configuration = sentenceConfiguration;
      TokenizedText = Tokenizer.Tokenize(sentence);
      PreProcessedTokens = TokenizedText.PreProcess(services.Vocab, services.DictLookup);

      Locations = [];

      var characterIndex = 0;
      for(var tokenIndex = 0; tokenIndex < PreProcessedTokens.Count; tokenIndex++)
      {
         var token = PreProcessedTokens[tokenIndex];
         Locations.Add(new TextAnalysisLocation(this, token, characterIndex, tokenIndex));
         characterIndex += token.Surface.Length;
      }

      StartLocation = Locations[0];
      ConnectNextAndPreviousToLocations();
      AnalysisStep1AnalyzeNonCompound();
      AnalysisStep2AnalyzeCompounds();
      AnalysisStep3RunDisplayAnalysisWithoutShadowingInformationSoThatAllValidMatchesAreDisplayedAndCanBeAccountedForInYieldingToUpcomingCompoundsReturnTrueOnChanges();
      if(AnalysisStep4SetInitialShadowingStateReturnTrueOnChanges())
      {
         AnalysisStep5CalculatePreferenceBetweenOverlappingDisplayCandidates();
      }

      IndexingWordVariants = [.. Locations.SelectMany(location => location.IndexingVariants)];
      DisplayWordVariants = [.. Locations.Select(location => location.DisplayVariant).OfType<CandidateWordVariant>()];
      ValidMatches = [.. IndexingWordVariants.SelectMany(variant => variant.ValidMatches)];
      DisplayMatches = [.. DisplayWordVariants.SelectMany(variant => variant.DisplayMatches)];
   }

   public static TextAnalysis FromText(AnalysisServices services, string text) => new(services, text, SentenceConfiguration.Empty());

   public List<string> AllWordsStrings() => [.. ValidMatches.Select(w => w.ParsedForm)];

   public override string ToString() => Text;

   void ConnectNextAndPreviousToLocations()
   {
      for(var tokenIndex = 0; tokenIndex < Locations.Count; tokenIndex++)
      {
         var location = Locations[tokenIndex];
         if(Locations.Count > tokenIndex + 1)
         {
            location.Next = Locations[tokenIndex + 1];
         }

         if(tokenIndex > 0)
         {
            location.Previous = Locations[tokenIndex - 1];
         }
      }
   }

   void AnalysisStep1AnalyzeNonCompound()
   {
      foreach(var location in Locations)
      {
         location.AnalysisStep1AnalyzeNonCompoundValidity();
      }
   }

   void AnalysisStep2AnalyzeCompounds()
   {
      foreach(var location in Locations)
      {
         location.AnalysisStep2AnalyzeCompoundValidity();
      }
   }

   void AnalysisStep3RunDisplayAnalysisWithoutShadowingInformationSoThatAllValidMatchesAreDisplayedAndCanBeAccountedForInYieldingToUpcomingCompoundsReturnTrueOnChanges()
   {
      foreach(var location in Locations)
      {
         location.AnalysisStep3RunDisplayAnalysisWithoutShadowingInformationSoThatAllValidMatchesAreDisplayedAndCanBeAccountedForInYieldingToUpcomingCompounds();
      }
   }

   bool AnalysisStep4SetInitialShadowingStateReturnTrueOnChanges()
   {
      var changeMade = false;
      foreach(var location in Locations)
      {
         if(location.AnalysisStep4SetInitialShadowingAndRecalculateDisplayWordsReturnTrueOnChanges())
         {
            changeMade = true;
         }
      }

      return changeMade;
   }

   void AnalysisStep5CalculatePreferenceBetweenOverlappingDisplayCandidates()
   {
      var changesMade = true;
      while(changesMade)
      {
         changesMade = false;
         foreach(var location in Locations)
         {
            if(location.AnalysisStep5UpdateShadowingAndRecalculateDisplayWordsReturnTrueOnChanges())
            {
               changesMade = true;
            }
         }
      }
   }
}
