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
                Question = note.Question.DisambiguationName,
                SourceAnswer = note.SourceAnswer.Value,
                UserAnswer = note.User.Answer.Value,
                ActiveAnswer = note.ActiveAnswer.Value,
                UserExplanation = note.User.Explanation.Value,
                UserExplanationLong = note.User.ExplanationLong.Value,
                UserMnemonic = note.User.Mnemonic.Value,
                UserCompounds = note.CompoundParts.AllRaw(),
                Readings = note.GetReadings(),
                PartsOfSpeech = note.PartsOfSpeech.RawStringValue(),
                SourceMnemonic = note.SourceMnemonic.Value,
                SourceReadingMnemonic = note.SourceReadingMnemonic.Value,
                AudioB = note.Audio.First.RawValue(),
                AudioG = note.Audio.Second.RawValue(),
                AudioTTS = note.Audio.Tts.RawValue(),
                Forms = note.Forms.EnteredFormsWithBrackets(),
                SentenceCount = note.MetaData.SentenceCount,
                TechnicalNotes = note.TechnicalNotes.Value,
                References = note.References.Value,
                MatchingRules = new VocabMatchingRulesSubData
                                {
                                   PrefixIsNot = rules.PrefixIsNot.OrderBy(s => s).ToList(),
                                   SuffixIsNot = rules.SuffixIsNot.OrderBy(s => s).ToList(),
                                   SurfaceIsNot = rules.SurfaceIsNot.OrderBy(s => s).ToList(),
                                   YieldToSurface = rules.YieldToSurface.OrderBy(s => s).ToList(),
                                   RequiredPrefix = rules.RequiredPrefix.OrderBy(s => s).ToList(),
                                },
                RelatedVocab = new VocabRelatedSubData
                               {
                                  ErgativeTwin = related.ErgativeTwin.Get(),
                                  DerivedFrom = related.DerivedFrom.Get(),
                                  PerfectSynonyms = related.PerfectSynonyms.Get().OrderBy(s => s).ToList(),
                                  Synonyms = related.Synonyms.Strings().OrderBy(s => s).ToList(),
                                  Antonyms = related.Antonyms.Strings().OrderBy(s => s).ToList(),
                                  ConfusedWith = related.ConfusedWith.Get().OrderBy(s => s).ToList(),
                                  SeeAlso = related.SeeAlso.Strings().OrderBy(s => s).ToList(),
                               },
                Image = note.Image.RawValue(),
                UserImage = note.UserImage.RawValue(),
                Tags = note.Tags.ToStringList(),
             };
   }
}
