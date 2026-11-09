using System;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// One of a sentence's parsed words, a <see cref="ParsedMatch"/>, written out for a human to read: with how it's matched spelled out, and the<br/>
/// vocab it's matched to by its question as well as its id. As <see cref="ParsedWordsChange"/> reports it, and as a parsing case, the<br/>
/// specifications' <c>ParsingCase</c>, expects it.
/// </summary>
/// <param name="Form">The word as it's parsed: <see cref="ParsedMatch.ParsedForm"/>.</param>
/// <param name="StartIndex">Where in the sentence the word starts, in characters.</param>
/// <param name="Variant">
/// "surface" for a word matched by its form as the sentence has it, <see cref="ParsedMatch.SurfaceVariant"/>; "base" for one matched by its<br/>
/// dictionary form, <see cref="ParsedMatch.BaseVariant"/>.
/// </param>
/// <param name="IsDisplayed">Whether the sentence displays the word.</param>
/// <param name="Vocab">The question of the vocab the word is matched to. Null when it's matched to no vocab, or to a vocab the collection doesn't have.</param>
/// <param name="VocabId">The id of the vocab the word is matched to. Null when it's matched to no vocab, but only to the dictionary.</param>
public record ParsedWordReported(string Form, int StartIndex, string Variant, bool IsDisplayed, string? Vocab, string? VocabId)
{
   const string SurfaceVariant = "surface";
   const string BaseVariant = "base";

   /// <summary><paramref name="word"/> written out to read, naming the vocab it's matched to by its question in <paramref name="collection"/>.</summary>
   public static ParsedWordReported Of(ParsedMatch word, JPCollection collection) =>
      new(word.ParsedForm,
          word.StartIndex,
          word.Variant == ParsedMatch.SurfaceVariant ? SurfaceVariant : BaseVariant,
          word.IsDisplayed,
          word.VocabId is {} vocabId ? collection.NoteFromNoteId(vocabId)?.GetQuestion() : null,
          word.VocabId?.ToString());

   /// <summary>The parsed word this reports, without the question of its vocab, which a parsed word doesn't hold.</summary>
   public ParsedMatch ToParsedMatch() =>
      new(Variant switch
          {
             SurfaceVariant => ParsedMatch.SurfaceVariant,
             BaseVariant => ParsedMatch.BaseVariant,
             _ => throw new InvalidOperationException($"The parsed word {this} has the variant \"{Variant}\": a parsed word's is \"{SurfaceVariant}\" or \"{BaseVariant}\".")
          },
          StartIndex,
          IsDisplayed,
          Form,
          VocabId != null ? new VocabId(Guid.Parse(VocabId)) : null);

   /// <summary>The word on one line, such as <c>いただけない at 7, surface, displayed, vocab いただけない (460bb083-0019-4dc6-8591-0bdfcaec4ce8)</c>.</summary>
   public override string ToString() => $"{Form} at {StartIndex}, {Variant}, {(IsDisplayed ? "displayed" : "not displayed")}, {VocabMatched}";

   string VocabMatched => VocabId == null ? "no vocab" : $"vocab {Vocab ?? "the collection doesn't have"} ({VocabId})";
}
