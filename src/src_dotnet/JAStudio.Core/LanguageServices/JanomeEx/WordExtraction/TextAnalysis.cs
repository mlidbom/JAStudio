using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

public sealed class TextAnalysis
{
   public const string Version = "text_analysis_0.1";

   static readonly JNTokenizer Tokenizer = JNTokenizer.GetInstance();

   public AnalysisServices Services { get; }
   public bool ForUI { get; }
   public string Text { get; }
   public SentenceConfiguration Configuration { get; }
   public JNTokenizedText TokenizedText { get; }
   public List<IAnalysisToken> PreProcessedTokens { get; }
   public List<TextAnalysisLocation> Locations { get; }
   public TextAnalysisLocation StartLocation { get; }
   public List<CandidateWordVariant> IndexingWordVariants { get; }
   public List<CandidateWordVariant> DisplayWordVariants { get; }
   public List<Match> ValidMatches { get; }
   public List<Match> DisplayMatches { get; }

   public TextAnalysis(AnalysisServices services, string sentence, SentenceConfiguration sentenceConfiguration, bool forUI = false)
   {
      Services = services;
      ForUI = forUI;
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

      IndexingWordVariants = Locations
                            .SelectMany(location => location.IndexingVariants)
                            .ToList();

      DisplayWordVariants = Locations
                           .SelectMany(location => location.DisplayVariants)
                           .ToList();

      ValidMatches = IndexingWordVariants
                    .SelectMany(variant => variant.ValidMatches)
                    .ToList();

      DisplayMatches = DisplayWordVariants
                      .SelectMany(variant => variant.DisplayMatches)
                      .ToList();
   }

   public static TextAnalysis FromText(AnalysisServices services, string text) => new(services, text, SentenceConfiguration.Empty());

   public List<string> AllWordsStrings()
   {
      return ValidMatches.Select(w => w.ParsedForm).ToList();
   }

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
