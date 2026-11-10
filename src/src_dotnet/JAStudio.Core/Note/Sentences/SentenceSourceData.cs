using System.Linq;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Sentences;

/// <summary>
/// What a <see cref="SentenceNote"/>'s source, such as Immersion Kit or a JLab deck, gave it when it was imported:<br/>
/// its question, answer and comments, the id it has there, and its audio and screenshots.
/// </summary>
/// <remarks>
/// The source's question and answer are shown while the sentence has no <see cref="SentenceNote.CorpusQuestion"/> or <see cref="SentenceNote.CorpusAnswer"/><br/>
/// of its own. The source's comments are shown beside the sentence's own <see cref="SentenceNote.Comments"/>: neither replaces the other.
/// </remarks>
public class SentenceSourceData
{
   public SentenceSourceData(SentenceSourceDataDto? data, NoteGuard guard)
   {
      Question = new WritableStringValue(data?.Question ?? string.Empty, guard);
      Answer = new WritableStringValue(data?.Answer ?? string.Empty, guard);
      Comments = new WritableStringValue(data?.Comments ?? string.Empty, guard);
      ExternalId = new WritableStringValue(data?.ExternalId ?? string.Empty, guard);
      Audio = new AudioReferenceList(data?.Audio.Select(it => it.ToReference()) ?? [], guard);
      Screenshots = new MediaReferenceList<ImageReference>(data?.Screenshots.Select(it => it.ToReference()) ?? [], guard);
   }

   internal SentenceSourceDataDto ToDto() =>
      new()
      {
         Question = Question.Value,
         Answer = Answer.Value,
         Comments = Comments.Value,
         ExternalId = ExternalId.Value,
         Audio = [..Audio.All.Select(AudioReferenceDto.From)],
         Screenshots = [..Screenshots.All.Select(ImageReferenceDto.From)],
      };

   /// <summary>The source's question, shown while the sentence has no <see cref="SentenceNote.CorpusQuestion"/>.</summary>
   public WritableStringValue Question { get; }

   /// <summary>The source's answer, shown while the sentence has no <see cref="SentenceNote.CorpusAnswer"/>.</summary>
   public WritableStringValue Answer { get; }

   /// <summary>The source's comments on the sentence, shown beside the sentence's own <see cref="SentenceNote.Comments"/>.</summary>
   public WritableStringValue Comments { get; }

   /// <summary>The sentence's id in its source, such as Immersion Kit's <c>anime_the_garden_of_words_000000346</c>: an import uses it to recognize sentences it has imported before.</summary>
   public WritableStringValue ExternalId { get; }

   /// <summary>The audio the source gave the sentence, such as the line spoken in the anime it's from.</summary>
   public AudioReferenceList Audio { get; }

   /// <summary>The screenshots the source gave the sentence, such as a frame of the anime scene it's from, shown on its card.</summary>
   public MediaReferenceList<ImageReference> Screenshots { get; }
}
