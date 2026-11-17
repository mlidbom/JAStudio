using System.Linq;
using Compze.Teventive;
using Compze.Teventive.Taggregates.BaseClasses;
using JAStudio.Core.Note.Media;
using JAStudio.Core.Note.Sentences.Tevents;
using JAStudio.Core.Note.Tevents;
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
/// <remarks>
/// A component of its sentence: its text fields publish tevents of their own, <see cref="ISentenceTevent.SourceData"/>, which its appliers apply.<br/>
/// Its audio and screenshots still change through the sentence's <see cref="NoteGuard"/>.
/// </remarks>
public class SentenceSourceData : TeventiveComponent<SentenceNote, INoteTevent, NoteTevent, SentenceSourceData, ISentenceTevent.SourceData, SentenceTevent.SourceData>, ITeventRouted
{
   static readonly TeventRouting<SentenceSourceData> Appliers = TeventRouting<SentenceSourceData>.DeclareAppliers(it => it
     .For<ISentenceTevent.SourceData.PropertyUpdated.Question>(static (sourceData, tevent) => sourceData.Question = tevent.Question)
     .For<ISentenceTevent.SourceData.PropertyUpdated.Answer>(static (sourceData, tevent) => sourceData.Answer = tevent.Answer)
     .For<ISentenceTevent.SourceData.PropertyUpdated.Comments>(static (sourceData, tevent) => sourceData.Comments = tevent.Comments)
     .For<ISentenceTevent.SourceData.PropertyUpdated.ExternalId>(static (sourceData, tevent) => sourceData.ExternalId = tevent.ExternalId));

   TeventRouting ITeventRouted.TeventAppliers => Appliers;

   readonly SentenceNote _sentence;

   public SentenceSourceData(SentenceNote sentence, SentenceSourceDataDto? data, NoteGuard guard) : base(sentence)
   {
      _sentence = sentence;
      Question = data?.Question ?? string.Empty;
      Answer = data?.Answer ?? string.Empty;
      Comments = data?.Comments ?? string.Empty;
      ExternalId = data?.ExternalId ?? string.Empty;
      Audio = new AudioReferenceList(data?.Audio.Select(it => it.ToReference()) ?? [], guard);
      Screenshots = new MediaReferenceList<ImageReference>(data?.Screenshots.Select(it => it.ToReference()) ?? [], guard);
   }

   internal SentenceSourceDataDto ToDto() =>
      new()
      {
         Question = Question,
         Answer = Answer,
         Comments = Comments,
         ExternalId = ExternalId,
         Audio = [..Audio.All.Select(AudioReferenceDto.From)],
         Screenshots = [..Screenshots.All.Select(ImageReferenceDto.From)],
      };

   /// <summary>The source's question, shown while the sentence has no <see cref="SentenceNote.CorpusQuestion"/>.</summary>
   public string Question { get; private set; }

   /// <summary>The source's answer, shown while the sentence has no <see cref="SentenceNote.CorpusAnswer"/>.</summary>
   public string Answer { get; private set; }

   /// <summary>The source's comments on the sentence, shown beside the sentence's own <see cref="SentenceNote.Comments"/>.</summary>
   public string Comments { get; private set; }

   /// <summary>The sentence's id in its source, such as Immersion Kit's <c>anime_the_garden_of_words_000000346</c>: an import uses it to recognize sentences it has imported before.</summary>
   public string ExternalId { get; private set; }

   /// <summary>Gives the sentence <paramref name="question"/>, trimmed, as its source's <see cref="Question"/>.</summary>
   public void SetQuestion(string question) => _sentence.PublishIfChanged(Question, question.Trim(), static it => new SentenceTevent.SourceData.SourceQuestionUpdated(it));

   /// <summary>Gives the sentence <paramref name="answer"/>, trimmed, as its source's <see cref="Answer"/>.</summary>
   public void SetAnswer(string answer) => _sentence.PublishIfChanged(Answer, answer.Trim(), static it => new SentenceTevent.SourceData.SourceAnswerUpdated(it));

   /// <summary>Gives the sentence <paramref name="comments"/>, trimmed, as its source's <see cref="Comments"/>.</summary>
   public void SetComments(string comments) => _sentence.PublishIfChanged(Comments, comments.Trim(), static it => new SentenceTevent.SourceData.SourceCommentsUpdated(it));

   /// <summary>Gives the sentence <paramref name="externalId"/>, trimmed, as its source's <see cref="ExternalId"/>.</summary>
   public void SetExternalId(string externalId) =>
      _sentence.PublishIfChanged(ExternalId, externalId.Trim(), static it => new SentenceTevent.SourceData.SourceExternalIdUpdated(it));

   /// <summary>The audio the source gave the sentence, such as the line spoken in the anime it's from.</summary>
   public AudioReferenceList Audio { get; }

   /// <summary>The screenshots the source gave the sentence, such as a frame of the anime scene it's from, shown on its card.</summary>
   public MediaReferenceList<ImageReference> Screenshots { get; }
}
