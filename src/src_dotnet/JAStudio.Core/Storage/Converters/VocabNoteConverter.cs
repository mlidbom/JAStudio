using System.Linq;
using JAStudio.Core.Storage.Dto;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Converters;

static class VocabNoteConverter
{
   public static VocabDto ToDto(VocabNote note)
   {
      var rules = note.MatchingConfiguration.ConfigurableRules;
      var related = note.RelatedNotes;

      return new VocabDto
             {
                Id = note.GetId().Value,
                Question = note.Question.ToDto(),
                SourceAnswer = note.SourceData.Answer.Value,
                UserAnswer = note.CorpusAnswer.Value,
                ActiveAnswer = note.GetAnswer(),
                UserExplanation = note.Explanation.Value,
                UserExplanationLong = note.ExplanationLong.Value,
                UserMnemonic = note.Mnemonic.Value,
                CompoundParts = new VocabCompoundPartsDto { Primary = note.CompoundParts.Primary(), Alternative = note.CompoundParts.Alternative() },
                Readings = note.GetReadings(),
                PartsOfSpeech = note.PartsOfSpeech.RawStringValue(),
                SourceMnemonic = note.SourceData.Mnemonic.Value,
                SourceReadingMnemonic = note.SourceData.ReadingMnemonic.Value,
                AudioB = note.Audio.First.RawValue(),
                AudioG = note.Audio.Second.RawValue(),
                AudioTTS = note.Audio.Tts.RawValue(),
                Forms = [.. note.Forms.EnteredForms().Select(it => new EnteredFormDto { Text = it.Text, IsOwned = it.IsOwned })],
                SentenceCount = note.MetaData.SentenceCount,
                TechnicalNotes = note.TechnicalNotes.Value,
                References = note.References.Value,
                MatchingRules = new VocabMatchingRulesDto
                                {
                                   PrefixIsNot = [.. rules.PrefixIsNot.OrderBy(s => s)],
                                   SuffixIsNot = [.. rules.SuffixIsNot.OrderBy(s => s)],
                                   SurfaceIsNot = [.. rules.SurfaceIsNot.OrderBy(s => s)],
                                   YieldToSurface = [.. rules.YieldToSurface.OrderBy(s => s)],
                                   RequiredPrefix = [.. rules.RequiredPrefix.OrderBy(s => s)],
                                },
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
                Tags = note.Tags.ToStringList(),
             };
   }
}
