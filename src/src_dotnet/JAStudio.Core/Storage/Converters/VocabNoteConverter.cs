using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Converters;

static class VocabNoteConverter
{
   public static VocabDto ToDto(VocabNote note)
   {
      var related = note.RelatedNotes;

      return new VocabDto
             {
                Id = note.GetId().Value,
                Question = note.Question.ToDto(),
                CorpusAnswer = note.CorpusAnswer.Value,
                Explanation = note.Explanation.Value,
                ExplanationLong = note.ExplanationLong.Value,
                Mnemonic = note.Mnemonic.Value,
                CompoundParts = new VocabCompoundPartsDto { Primary = ToDtos(note.CompoundParts.Primary()), Alternative = ToDtos(note.CompoundParts.Alternative()) },
                Readings = note.GetReadings(),
                PartsOfSpeech = note.PartsOfSpeech.ToDto(),
                Register = note.Register.ToDto(),
                UsuallyWrittenInKana = note.UsuallyWrittenInKana.ToDto(),
                //wip: slice 9's step 3 stores the media lists themselves, rather than in the media fields of today (TodaysMediaFields).
                AudioB = AnkiMediaMarkup.SoundTagsFor(note.AllAudio.Recordings.Take(1)),
                AudioG = AnkiMediaMarkup.SoundTagsFor(note.AllAudio.Recordings.Skip(1)),
                AudioTTS = AnkiMediaMarkup.SoundTagsFor(note.AllAudio.TextToSpeech),
                Image = AnkiMediaMarkup.ImgTagsFor(note.SourceData.Images.All),
                UserImage = AnkiMediaMarkup.ImgTagsFor(note.Images.All),
                Forms = [.. note.Forms.EnteredForms().Select(it => new EnteredFormDto { Text = it.Text, IsOwned = it.IsOwned })],
                SentenceCount = note.MetaData.SentenceCount,
                TechnicalNotes = note.TechnicalNotes.Value,
                References = note.References.Value,
                Matching = note.MatchingConfiguration.ToDto(),
                RelatedVocab = new VocabRelatedDto
                               {
                                  ErgativeTwins = InAFixedOrder(related.ErgativeTwins.Words),
                                  DerivedFrom = related.DerivedFrom.Word?.ToDto(),
                                  PerfectSynonyms = InAFixedOrder(related.PerfectSynonyms.Words),
                                  Synonyms = InAFixedOrder(related.Synonyms.Words),
                                  Antonyms = InAFixedOrder(related.Antonyms.Words),
                                  ConfusedWith = InAFixedOrder(related.ConfusedWith.Words),
                                  SeeAlso = InAFixedOrder(related.SeeAlso.Words)
                               },
                SourceData = note.SourceData.ToDto(),
                Tags = note.Tags.ToStringList()
             };
   }

   static List<WordReferenceDto> ToDtos(IEnumerable<WordReference> references) => [.. references.Select(it => it.ToDto())];

   /// <summary>A set of references in a fixed order, by their names, so that the same set always writes the same file.</summary>
   static List<WordReferenceDto> InAFixedOrder(IEnumerable<WordReference> references)
   {
      //review: This orders by the current culture's comparison, as the related vocab's strings were before they were word-references, while
      //   WordReference.InAFixedOrder, which the alternative compound parts are stored in, orders ordinally. Should the corpus store both ordinally? Since the
      //   current culture is the machine's, the stored order could differ between machines. Switching reorders the sets whose orders differ, once.
      return ToDtos(references.OrderBy(it => it.DisambiguationName));
   }
}
