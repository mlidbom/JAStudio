using System.Collections.Generic;
using System.Text.Json.Serialization;
using JAStudio.Core.UI.Web.Sentence;

namespace JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;

/// <summary>
/// A word the sentence breakdown displays, written out to read: as <see cref="MatchViewModel"/> shows it when the breakdown isn't in edit mode,<br/>
/// with the vocab it's matched to by its question as well as its id.
/// </summary>
/// <param name="Form">The word as it's parsed: <see cref="MatchViewModel.ParsedForm"/>.</param>
/// <param name="StartIndex">Where in the sentence the word starts, in characters.</param>
/// <param name="Variant">"surface" for a word matched by its form as the sentence has it; "base" for one matched by its dictionary form.</param>
/// <param name="ShownAs">
/// The form the breakdown shows beside the word, when it isn't the word's own (<see cref="MatchViewModel.FormShownBesideTheWordOutsideEditMode"/>): such as う for<br/>
/// る, the vocab's form, or <c>[MISSING]</c> for a word matched to nothing. Null when it shows none.
/// </param>
/// <param name="IsEmergencyDisplayed">
/// Whether the breakdown shows the word though its match isn't valid, because it's a whole token and nothing else covers that part of the<br/>
/// sentence.
/// </param>
/// <param name="Vocab">The question of the vocab the word is matched to. Null when it's matched to no vocab. Not compared: it's there to read.</param>
/// <param name="VocabId">The id of the vocab the word is matched to. Null when it's matched to no vocab.</param>
public sealed record BreakdownWord(string Form, int StartIndex, string Variant, string? ShownAs, bool IsEmergencyDisplayed, string? Vocab, string? VocabId)
{
   /// <summary>The <see cref="Variant"/> of a word matched by its form as the sentence has it.</summary>
   public const string SurfaceVariant = "surface";

   /// <summary>The <see cref="Variant"/> of a word matched by its dictionary form.</summary>
   public const string BaseVariant = "base";

   /// <summary>Whether the word is matched by its form as the sentence has it, not by its dictionary form.</summary>
   [JsonIgnore] public bool IsMatchedByItsSurface => Variant == SurfaceVariant;

   public static BreakdownWord Of(MatchViewModel displayedMatch) =>
      new(displayedMatch.ParsedForm,
          displayedMatch.Match.StartIndex,
          displayedMatch.Match.Variant.IsSurface ? SurfaceVariant : BaseVariant,
          displayedMatch.FormShownBesideTheWordOutsideEditMode,
          displayedMatch.Match.IsEmergencyDisplayed,
          displayedMatch.VocabMatch?.Vocab.GetQuestion(),
          displayedMatch.VocabMatch?.Vocab.GetId().ToString());

   /// <summary>Whether this is the same word as <paramref name="other"/>, displayed the same way, whatever the question of its vocab.</summary>
   public bool IsTheSameWordAs(BreakdownWord other) => this with { Vocab = null } == other with { Vocab = null };

   /// <summary>The line that says a word is displayed that the parsing case doesn't expect.</summary>
   public const string DisplayedButNotExpected = "displayed, but expected not displayed";

   /// <summary>The line that says this word, which the parsing case expects, isn't displayed.</summary>
   public string ExpectedButNotDisplayed => $"expected, but not displayed: {FormWithItsVocab}";

   /// <summary>
   /// How this word, displayed, differs from <paramref name="expected"/>, the word the parsing case expects in its place, a line per difference:<br/>
   /// for another word, that this one isn't expected and the other isn't displayed; for the same word, what is displayed otherwise, such as the vocab<br/>
   /// it's matched to. Empty when it's the same word, displayed the same way (<see cref="IsTheSameWordAs"/>).
   /// </summary>
   public IReadOnlyList<string> HowItDiffersFrom(BreakdownWord expected)
   {
      if(Form != expected.Form || StartIndex != expected.StartIndex)
         return [DisplayedButNotExpected, expected.ExpectedButNotDisplayed];

      List<string> differences = [];
      if(VocabId != expected.VocabId)
         differences.Add($"matched to {VocabNamed}, but expected matched to {expected.VocabNamed}");
      if(Variant != expected.Variant)
         differences.Add($"matched by {VariantDescribed}, but expected matched by {expected.VariantDescribed}");
      if(ShownAs != expected.ShownAs)
         differences.Add($"shows {ShownAs ?? "nothing"} beside the word, but expected {expected.ShownAs ?? "nothing"}");
      if(IsEmergencyDisplayed != expected.IsEmergencyDisplayed)
         differences.Add(IsEmergencyDisplayed
                            ? "displayed though its match isn't valid, but expected displayed by a valid match"
                            : "displayed by a valid match, but expected displayed though its match isn't valid");
      return differences;
   }

   /// <summary>The word, followed by the question of its vocab when that isn't the word as written, such as <c>走っ (走る)</c>.</summary>
   string FormWithItsVocab => Vocab is null || Vocab == Form ? Form : $"{Form} ({Vocab})";

   string VocabNamed => VocabId == null ? "no vocab" : $"the vocab {Vocab ?? VocabId}";

   string VariantDescribed => IsMatchedByItsSurface ? "its form in the sentence" : "its dictionary form";

   /// <summary>The word on one line, such as <c>る at 3, surface, shown as う, vocab う (dcca0586-d8a4-4281-9f5f-a50f58a75fb2)</c>.</summary>
   public override string ToString() =>
      $"{Form} at {StartIndex}, {Variant}{(ShownAs != null ? $", shown as {ShownAs}" : "")}{(IsEmergencyDisplayed ? ", emergency displayed" : "")}, {VocabMatched}";

   string VocabMatched => VocabId == null ? "no vocab" : $"vocab {Vocab} ({VocabId})";
}
