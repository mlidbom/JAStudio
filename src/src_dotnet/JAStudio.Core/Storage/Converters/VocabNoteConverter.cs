using System.Linq;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage.Converters;

static class VocabNoteConverter
{
   public static VocabData ToCorpusData(VocabNote note)
   {
      var rules = note.MatchingConfiguration.ConfigurableRules;
      var related = note.RelatedNotes;

      return new VocabData
             {
                Id = note.GetId().Value,
                Question = note.Question.ToCorpusData(),
                SourceAnswer = note.SourceAnswer.Value,
                UserAnswer = note.User.Answer.Value,
                ActiveAnswer = note.ActiveAnswer.Value,
                UserExplanation = note.User.Explanation.Value,
                UserExplanationLong = note.User.ExplanationLong.Value,
                UserMnemonic = note.User.Mnemonic.Value,
                CompoundParts = new VocabCompoundPartsSubData { Primary = note.CompoundParts.Primary(), Alternative = note.CompoundParts.Alternative() },
                Readings = note.GetReadings(),
                PartsOfSpeech = note.PartsOfSpeech.RawStringValue(),
                SourceMnemonic = note.SourceMnemonic.Value,
                SourceReadingMnemonic = note.SourceReadingMnemonic.Value,
                AudioB = note.Audio.First.RawValue(),
                AudioG = note.Audio.Second.RawValue(),
                AudioTTS = note.Audio.Tts.RawValue(),
                Forms = [.. note.Forms.EnteredForms().Select(it => new EnteredFormSubData { Text = it.Text, IsOwned = it.IsOwned })],
                SentenceCount = note.MetaData.SentenceCount,
                TechnicalNotes = note.TechnicalNotes.Value,
                References = note.References.Value,
                MatchingRules = new VocabMatchingRulesSubData
                                {
                                   PrefixIsNot = [.. rules.PrefixIsNot.OrderBy(s => s)],
                                   SuffixIsNot = [.. rules.SuffixIsNot.OrderBy(s => s)],
                                   SurfaceIsNot = [.. rules.SurfaceIsNot.OrderBy(s => s)],
                                   YieldToSurface = [.. rules.YieldToSurface.OrderBy(s => s)],
                                   RequiredPrefix = [.. rules.RequiredPrefix.OrderBy(s => s)],
                                },
                RelatedVocab = new VocabRelatedSubData
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
