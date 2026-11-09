using System.Collections.Generic;
using System.Linq;
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
                AudioB = note.Audio.First.RawValue(),
                AudioG = note.Audio.Second.RawValue(),
                AudioTTS = note.Audio.Tts.RawValue(),
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
                                  SeeAlso = InAFixedOrder(related.SeeAlso.Words),
                               },
                Image = note.Image.RawValue(),
                UserImage = note.UserImage.RawValue(),
                SourceData = note.SourceData.ToDto(),
                Tags = note.Tags.ToStringList(),
             };
   }

   static List<WordReferenceDto> ToDtos(IEnumerable<WordReference> references) => [.. references.Select(it => it.ToDto())];

   /// <summary>A set of references in a fixed order, by their names, so that the same set always writes the same file.</summary>
   static List<WordReferenceDto> InAFixedOrder(IEnumerable<WordReference> references) => ToDtos(references.OrderBy(it => it.DisambiguationName));
}
