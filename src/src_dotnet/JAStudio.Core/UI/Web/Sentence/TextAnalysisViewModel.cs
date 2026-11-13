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

   public TextAnalysisViewModel(TextAnalysis textAnalysis, Settings settings, VocabCollection vocab)
   {
      Analysis = textAnalysis;
      CandidateWords = textAnalysis.IndexingWordVariants
                                   .Select(v => new CandidateWordVariantViewModel(v, settings, vocab))
                                   .ToList();

      var matches = CandidateWords.SelectMany(variantVm => variantVm.Matches).ToList();
      DisplayedMatches = matches.Where(match => match.IsDisplayed).ToList();
   }
}
