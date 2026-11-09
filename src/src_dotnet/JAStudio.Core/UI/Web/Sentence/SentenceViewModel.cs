using System.Collections.Generic;
using JAStudio.Core.Configuration;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Core.UI.Web.Sentence;

public class SentenceViewModel
{
   public SentenceNote Sentence { get; }
   public TextAnalysisViewModel Analysis { get; }
   public List<MatchViewModel> DisplayedMatches { get; }

   public SentenceViewModel(SentenceNote sentence, Settings settings, VocabCollection vocab)
   {
      Sentence = sentence;
      Analysis = new TextAnalysisViewModel(sentence.CreateBreakdownAnalysis(settings.BreakdownParsingOptions()), settings, vocab);
      DisplayedMatches = Analysis.DisplayedMatches;
   }
}
