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
                CompoundParts = new VocabCompoundPartsDto { Primary = note.CompoundParts.Primary(), Alternative = note.CompoundParts.Alternative() },
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
                                  ErgativeTwin = related.ErgativeTwin.Get(),
                                  DerivedFrom = related.DerivedFrom.Get(),
                                  PerfectSynonyms = [.. related.PerfectSynonyms.Get().OrderBy(s => s)],
                                  Synonyms = [.. related.Synonyms.Strings().OrderBy(s => s)],
                                  Antonyms = [.. related.Antonyms.Strings().OrderBy(s => s)],
                                  ConfusedWith = [.. related.ConfusedWith.Get().OrderBy(s => s)],
                                  SeeAlso = [.. related.SeeAlso.Strings().OrderBy(s => s)],
                               },
                Image = note.Image.RawValue(),
                UserImage = note.UserImage.RawValue(),
                SourceData = note.SourceData.ToDto(),
                Tags = note.Tags.ToStringList(),
             };
   }
}
