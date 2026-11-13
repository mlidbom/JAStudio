using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction.Matches;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Matching.Checking;

/// <summary>
/// One of the words in the sentence a vocab's match-condition is checked against (<see cref="WordToCheckAgainstViewModel"/>): a word that is one of the<br/>
/// vocab's forms, with the vocab's match of it, listed with whether it matches the vocab, and why not.
/// </summary>
sealed class WordInTheSentenceViewModel
{
   internal WordInTheSentenceViewModel(VocabMatch match) => Match = match;

   /// <summary>The vocab's match of the word, as the sentence's analysis made it, valid or not.</summary>
   public VocabMatch Match { get; }

   /// <summary>The word, as the vocab's form it is, the tree is checked against.</summary>
   public CandidateWordVariant Word => Match.Variant;

   /// <summary>
   /// The word as the list shows it: where in the sentence it is, and whether it matches the vocab, such as "えた, at 2: matches the vocab", or<br/>
   /// "食べ as 食べる, at 0: doesn't match: !PrecedingToken.IsAGodanVerb", for a word matched by its base form.
   /// </summary>
   public string Text => $"{TheWordAsTheVocabsForm}, at {Word.StartIndex}: {WhetherItMatchesTheVocab}";

   /// <summary>Whether this is the same word as <paramref name="word"/>, in another analysis of the sentence: the same form at the same place.</summary>
   internal bool IsTheSameWordAs(CandidateWordVariant word) => Word.StartIndex == word.StartIndex && Word.Form == word.Form;

   string TheWordAsTheVocabsForm => Word.IsSurface ? Word.Form : $"{Word.Word.SurfaceForm} as {Word.Form}";

   string WhetherItMatchesTheVocab => Match.IsValid ? "matches the vocab" : $"doesn't match: {string.Join("; ", Match.FailureReasons)}";
}
