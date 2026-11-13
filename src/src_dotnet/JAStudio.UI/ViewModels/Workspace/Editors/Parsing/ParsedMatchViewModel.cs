using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.Matches;
using JAStudio.Core.LanguageServices.Parsing.WordExtraction.MatchTrees;
using JAStudio.UI.ViewModels.Workspace.NotePages.Sentences;

namespace JAStudio.UI.ViewModels.Workspace.Editors.Parsing;

/// <summary>
/// A row of a sentence's parsing view (<see cref="SentenceParsingViewModel"/>): a match, shown or not, with the word as it's written in the sentence, what<br/>
/// it matched, its readings and answer, and the reasons it doesn't match or isn't shown. A match not shown is greyed, and an invalid one's word is red.
/// </summary>
sealed class ParsedMatchViewModel
{
   /// <summary>How far a covered match is indented under the match that covers it, in pixels.</summary>
   const double IndentationOfACoveredMatch = 30;

   ParsedMatchViewModel(Match match, bool isShown, double indentation)
   {
      Match = match;
      IsShown = isShown;
      Indentation = indentation;
      Reasons = [.. match.FailureReasons.Select(it => new MatchReasonViewModel(it, MakesTheMatchIncorrect: true)),
                 .. match.HidingReasons.Select(it => new MatchReasonViewModel(it, MakesTheMatchIncorrect: false))];
   }

   /// <summary>The row of a match at the top of the tree: a shown match, or one no shown match covers.</summary>
   public static ParsedMatchViewModel AtTheTop(MatchTreeNode node) => new(node.Match, node.IsShown, indentation: 0);

   /// <summary>The row of a match not shown that a shown match covers, indented under it.</summary>
   public static ParsedMatchViewModel Covered(Match match) => new(match, isShown: false, IndentationOfACoveredMatch);

   /// <summary>The match, which the row's right-click menu is about.</summary>
   public Match Match { get; }

   /// <summary>Whether the sentence breakdown shows the match. A match not shown is greyed.</summary>
   public bool IsShown { get; }

   /// <summary>Whether the match is invalid: it doesn't match, for the <see cref="Reasons"/> marked as making it incorrect. Its word is then red.</summary>
   public bool IsInvalid => !Match.IsValid;

   /// <summary>Whether the sentence highlights the match.</summary>
   public bool IsHighlighted => Match.IsHighlighted;

   /// <summary>How far the row is indented: not at all at the top of the tree, more for a match covered by the one above it.</summary>
   public double Indentation { get; }

   /// <summary>The word as it's written in the sentence, followed by the form it was matched as when that's its base form, such as "食べ as 食べる".</summary>
   public string Word => Match.Variant.IsSurface ? Match.Variant.Form : $"{Match.Word.SurfaceForm} as {Match.Variant.Form}";

   /// <summary>What the word matched: a vocab, by its question, unless that's the word's form, the dictionary, or nothing at all.</summary>
   public string? Matched =>
      Match switch
      {
         VocabMatch vocabMatch => vocabMatch.Vocab.Question.DisambiguationName is var question && question != Match.Variant.Form ? question : null,
         DictionaryMatch => "(dictionary)",
         _ => "(nothing)"
      };

   /// <summary>The readings of what matched, unless they're the word's form or what it matched, as for a word written in kana.</summary>
   public string? Readings => string.Join(", ", Match.Readings) is var readings && readings != Match.Variant.Form && readings != Matched ? readings : null;

   /// <summary>The answer of what matched, with its markup.</summary>
   public string Answer => Match.Answer;

   /// <summary>The reasons the word doesn't match, then those it isn't shown, as the matching gives them, such as <c>forbids::configured_hidden</c>.</summary>
   public IReadOnlyList<MatchReasonViewModel> Reasons { get; }
}
