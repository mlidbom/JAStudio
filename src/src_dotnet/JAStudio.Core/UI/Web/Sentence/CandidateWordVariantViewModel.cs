using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Configuration;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.UI.Web.Sentence;

public class CandidateWordVariantViewModel
{
   public CandidateWordVariant CandidateWord { get; }
   public bool IsDisplayWord { get; }
   public List<MatchViewModel> Matches { get; }
   public List<MatchViewModel> DisplayMatches { get; }
   public bool HasPerfectMatch { get; }
   public List<MatchViewModel> PrimaryDisplayForms { get; }

   public CandidateWordVariantViewModel(CandidateWordVariant variant, Settings settings, VocabCollection vocab)
   {
      CandidateWord = variant;
      IsDisplayWord = variant.Word.Analysis.DisplayWordVariants.Contains(variant);
      Matches = variant.Matches.Select(match => new MatchViewModel(this, match, settings, vocab)).ToList();
      DisplayMatches = Matches.Where(match => match.MatchIsDisplayed).ToList();
      HasPerfectMatch = Matches.Any(match => match.MatchOwnsForm && match.MatchIsDisplayed);

      PrimaryDisplayForms = Matches.Where(form => form.MatchIsDisplayed).ToList();
      Matches = Matches.OrderBy(matchVm => matchVm.MatchIsDisplayed ? 0 : 1).ToList();
   }

   public override string ToString()
   {
      var flags = new List<string> { CandidateWord.Form };
      if(IsDisplayWord) flags.Add("display_word");
      return string.Join(" ", flags);
   }
}
