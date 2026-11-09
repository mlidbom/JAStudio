using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.Sentences;

namespace JAStudio.Mcp.Tools.Corpus.NoteDiffs.ParsedWords;

/// <summary>One of a sentence's parsed words, a <see cref="ParsedMatch"/>, as <see cref="ParsedWordsChange"/> reports it, with the vocab it's matched to by its question.</summary>
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
   public static ParsedWordReported Of(ParsedMatch word, JPCollection collection) =>
      new(word.ParsedForm,
          word.StartIndex,
          word.Variant == ParsedMatch.SurfaceVariant ? "surface" : "base",
          word.IsDisplayed,
          word.VocabId is {} vocabId ? collection.NoteFromNoteId(vocabId)?.GetQuestion() : null,
          word.VocabId?.ToString());
}
