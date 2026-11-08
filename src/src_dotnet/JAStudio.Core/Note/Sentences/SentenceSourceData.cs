using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// What a <see cref="SentenceNote"/>'s source, such as Immersion Kit or a JLab deck, gave it when it was imported:<br/>
/// its question, answer and comments, and the id it has there.
/// </summary>
/// <remarks>
/// The source's question and answer are shown while the sentence has no <see cref="SentenceNote.CorpusQuestion"/> or <see cref="SentenceNote.CorpusAnswer"/><br/>
/// of its own. The source's comments are shown beside the sentence's own <see cref="SentenceNote.Comments"/>: neither replaces the other.
/// </remarks>
public class SentenceSourceData
{
   public SentenceSourceData(SentenceDto? data, NoteGuard guard)
   {
      Question = new WritableStringValue(data?.SourceQuestion ?? string.Empty, guard);
      Answer = new WritableStringValue(data?.SourceAnswer ?? string.Empty, guard);
      Comments = new WritableStringValue(data?.SourceComments ?? string.Empty, guard);
      ExternalId = new WritableStringValue(data?.ExternalId ?? string.Empty, guard);
   }

   /// <summary>The source's question, shown while the sentence has no <see cref="SentenceNote.CorpusQuestion"/>.</summary>
   public WritableStringValue Question { get; }

   /// <summary>The source's answer, shown while the sentence has no <see cref="SentenceNote.CorpusAnswer"/>.</summary>
   public WritableStringValue Answer { get; }

   /// <summary>The source's comments on the sentence, shown beside the sentence's own <see cref="SentenceNote.Comments"/>.</summary>
   public WritableStringValue Comments { get; }

   /// <summary>The sentence's id in its source, such as Immersion Kit's <c>anime_the_garden_of_words_000000346</c>: an import uses it to recognize sentences it has imported before.</summary>
   public WritableStringValue ExternalId { get; }
}
