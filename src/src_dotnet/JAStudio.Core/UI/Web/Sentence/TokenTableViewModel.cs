using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.Tokenizing;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.UI.Web.Sentence;

/// <summary>
/// A table of a sentence's tokens, which the sentence breakdown shows in edit mode (<see cref="TokenViewModel"/>): the tokens JAStudio's analysis<br/>
/// works with, and, when the pre-processing stage made some of its own from the tokenizer's, the tokenizer's as it gave them.
/// </summary>
public sealed class TokenTableViewModel
{
   TokenTableViewModel(string title, IEnumerable<IAnalysisToken> tokens)
   {
      Title = title;
      Tokens = [.. tokens.Select(it => new TokenViewModel(it))];
   }

   public string Title { get; }

   public List<TokenViewModel> Tokens { get; }

   /// <summary>
   /// The tables of <paramref name="analysis"/>'s tokens: "Tokens", those the analysis works with, and "The tokenizer's tokens", as the tokenizer gave<br/>
   /// them, when the pre-processing stage made some of its own from them, such as by splitting a token or correcting the tokenizer's mistake.
   /// </summary>
   public static List<TokenTableViewModel> Of(TextAnalysis analysis)
   {
      var tokens = analysis.PreProcessedTokens.ToList();
      return tokens.Any(it => !it.IsTheTokenizersOwn)
                ? [new TokenTableViewModel("Tokens", tokens), new TokenTableViewModel("The tokenizer's tokens", analysis.TokenizedText.Tokens)]
                : [new TokenTableViewModel("Tokens", tokens)];
   }
}
