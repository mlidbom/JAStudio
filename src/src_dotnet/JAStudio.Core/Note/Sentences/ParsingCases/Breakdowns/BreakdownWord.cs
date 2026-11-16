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

   /// <summary>The word on one line, such as <c>る at 3, surface, shown as う, vocab う (dcca0586-d8a4-4281-9f5f-a50f58a75fb2)</c>.</summary>
   public override string ToString() =>
      $"{Form} at {StartIndex}, {Variant}{(ShownAs != null ? $", shown as {ShownAs}" : "")}{(IsEmergencyDisplayed ? ", emergency displayed" : "")}, {VocabMatched}";

   string VocabMatched => VocabId == null ? "no vocab" : $"vocab {Vocab} ({VocabId})";
}
