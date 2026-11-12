using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;

namespace JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

public sealed class TextAnalysisLocation
{
   const int MaxLookahead = 12; // In my collection the longest so far is 9, so 12 seems a pretty good choice.

   public TextAnalysisLocation? Next { get; set; }
   public TextAnalysisLocation? Previous { get; set; }
   public IAnalysisToken Token { get; }
   public List<TextAnalysisLocation> IsShadowedBy { get; }
   public List<TextAnalysisLocation> Shadows { get; }
   public TextAnalysis Analysis { get; }
   public int TokenIndex { get; }
   public int CharacterStartIndex { get; }
   public int CharacterEndIndex { get; }

   public List<CandidateWordVariant> DisplayVariants { get; set; }
   public List<CandidateWordVariant> IndexingVariants { get; set; }
   public List<CandidateWord> CandidateWords { get; set; }
   public List<CandidateWord> DisplayWords { get; set; }

   public TextAnalysisLocation(TextAnalysis analysis, IAnalysisToken token, int characterStartIndex, int tokenIndex)
   {
      Token = token;
      IsShadowedBy = [];
      Shadows = [];
      Analysis = analysis;
      TokenIndex = tokenIndex;
      CharacterStartIndex = characterStartIndex;
      CharacterEndIndex = characterStartIndex + token.Surface.Length - 1;

      DisplayVariants = [];
      IndexingVariants = [];
      CandidateWords = [];
      DisplayWords = [];
   }

   public override string ToString()
   {
      var variants = string.Join("\n", IndexingVariants.Select(cand => cand.ToString()));
      return $@"
TextLocation('{CharacterStartIndex}-{CharacterEndIndex}, {Token.Surface} | {Token.BaseForm})
{variants}
";
   }

   public List<TextAnalysisLocation> ForwardList(int length = 99999)
   {
      var actualLength = System.Math.Min(length + 1, Analysis.Locations.Count - TokenIndex);
      return Analysis.Locations.GetRange(TokenIndex, actualLength);
   }

   public CandidateWord NonCompoundCandidate => CandidateWords[^1];

   public void AnalysisStep1AnalyzeNonCompoundValidity()
   {
      CandidateWords = Enumerable.Range(0, MostTokensACandidateWordStartingHereCanSpan())
                                 .Reverse()
                                 .Select(index => new CandidateWord(ForwardList(index)))
                                 .ToList();
      CandidateWords[^1].RunValidityAnalysis(); // the non-compound part needs to be completed first
   }

   /// <summary>
   /// The most tokens a candidate word starting at this location is built for: as many as the sentence has left, up to <see cref="MaxLookahead"/>,<br/>
   /// but no more than a word that can match anything spans.
   /// </summary>
   /// <remarks>
   /// A candidate word gets matches only from the vocab and the dictionary words that have one of its forms, and each of its forms, its surface<br/>
   /// and its base form, starts with the surface of all its tokens but the last. So once no vocab or dictionary word form starts with the surface<br/>
   /// of the tokens so far, no longer candidate word here can match anything. Building them all, 80% of the candidate words the analysis<br/>
   /// built were compounds that matched nothing (dev_docs/parsing-speed.md).
   /// </remarks>
   int MostTokensACandidateWordStartingHereCanSpan()
   {
      var mostTokensTheSentenceHasLeft = System.Math.Min(MaxLookahead, Analysis.Locations.Count - TokenIndex);
      var tokens = 1;
      var surfaceOfTheTokens = Token.Surface;
      while(tokens < mostTokensTheSentenceHasLeft && Analysis.Services.AVocabOrDictionaryWordFormStartsWith(surfaceOfTheTokens))
      {
         surfaceOfTheTokens += Analysis.Locations[TokenIndex + tokens].Token.Surface;
         tokens++;
      }

      return tokens;
   }

   public void AnalysisStep2AnalyzeCompoundValidity()
   {
      foreach(var range in CandidateWords.Take(CandidateWords.Count - 1)) // we already have the last one completed
      {
         range.RunValidityAnalysis();
      }

      IndexingVariants = CandidateWords
                        .SelectMany(cand => cand.IndexingVariants)
                        .ToList();
   }

   public bool RunDisplayAnalysisAndUpdateDisplayWordsPassTrueIfThereWereChanges()
   {
      var changesMade = false;
      foreach(var range in CandidateWords)
      {
         if(range.RunDisplayAnalysisPassTrueIfThereWereChanges())
         {
            changesMade = true;
         }
      }

      if(changesMade)
      {
         DisplayWords = CandidateWords.Where(it => it.DisplayVariants.Any()).ToList();
      }

      return changesMade;
   }

   public void AnalysisStep3RunDisplayAnalysisWithoutShadowingInformationSoThatAllValidMatchesAreDisplayedAndCanBeAccountedForInYieldingToUpcomingCompounds()
   {
      RunDisplayAnalysisAndUpdateDisplayWordsPassTrueIfThereWereChanges();
   }

   public bool AnalysisStep4SetInitialShadowingAndRecalculateDisplayWordsReturnTrueOnChanges()
   {
      var displayWordsUpdated = RunDisplayAnalysisAndUpdateDisplayWordsPassTrueIfThereWereChanges();
      UpdateShadowing();
      return displayWordsUpdated;
   }

   public bool AnalysisStep5UpdateShadowingAndRecalculateDisplayWordsReturnTrueOnChanges()
   {
      var displayWordsUpdated = RunDisplayAnalysisAndUpdateDisplayWordsPassTrueIfThereWereChanges();
      if(displayWordsUpdated)
      {
         UpdateShadowing();
      }

      return displayWordsUpdated;
   }

   public void UpdateShadowing()
   {
      if(DisplayWords.Any() && !DisplayWords[0].IsShadowed)
      {
         DisplayVariants = DisplayWords[0].DisplayVariants;
         var coveringForwardCount = DisplayWords[0].LocationCount - 1;
         foreach(var shadowed in ForwardList(coveringForwardCount).Skip(1))
         {
            shadowed.IsShadowedBy.Add(this);
            Shadows.Add(shadowed);
            shadowed.ClearShadowed();
         }
      } else
      {
         ClearShadowed();
      }
   }

   void ClearShadowed()
   {
      foreach(var shadowedShadowed in Shadows)
      {
         shadowedShadowed.IsShadowedBy.Remove(this);
      }

      Shadows.Clear();
   }

   public bool IsNextLocationInflectingWord() => Next != null && Next.IsInflectingWord();

   // todo: having this check here only means that marking a compound as an inflecting word has no effect, and figuring out why things are not working can be quite a pain
   public bool IsInflectingWord()
   {
      var vocab = Analysis.Services.Vocab.WithAnyFormIn([Token.BaseForm, Token.Surface]);
      return vocab.Any(voc => voc.MatchingConfiguration.Flags.IsInflectingWord.IsActive);
   }
}
