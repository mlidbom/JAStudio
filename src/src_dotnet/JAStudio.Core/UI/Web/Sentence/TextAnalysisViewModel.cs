using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.UI.Web.Sentence;

public class TextAnalysisViewModel
{
   public TextAnalysis Analysis { get; }
   public List<CandidateWordVariantViewModel> CandidateWords { get; }
   public List<MatchViewModel> DisplayedMatches { get; }

   /// <summary>
   /// The matches the breakdown displays when it isn't in edit mode, which lists every match: the words a person reads the sentence by, whatever the<br/>
   /// setting. The same as <see cref="DisplayedMatches"/> outside edit mode.
   /// </summary>
   public List<MatchViewModel> MatchesDisplayedOutsideEditMode { get; }

   public TextAnalysisViewModel(TextAnalysis textAnalysis, Settings settings, VocabCollection vocab)
   {
      Analysis = textAnalysis;
      CandidateWords = textAnalysis.IndexingWordVariants
                                   .Select(v => new CandidateWordVariantViewModel(v, settings, vocab))
                                   .ToList();

      var matches = CandidateWords.SelectMany(variantVm => variantVm.Matches).ToList();
      DisplayedMatches = matches.Where(match => match.IsDisplayed).ToList();
      MatchesDisplayedOutsideEditMode = matches.Where(match => match.IsDisplayedOutsideEditMode).ToList();
   }
}
