using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.JamdictEx;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteGeneratedData
{
   readonly DictLookup _dictLookup;
   internal VocabNoteGeneratedData(DictLookup dictLookup) => _dictLookup = dictLookup;

   public void UpdateGeneratedData(VocabNote vocab)
   {
      vocab.MetaData.SetSentenceCount(vocab.Sentences.All().Count);
      vocab.ActiveAnswer.Set(vocab.GetAnswer());
      vocab.RelatedNotes.PerfectSynonyms.PushAnswerToOtherSynonyms();

      var question = vocab.Question.WithoutNoiseCharacters.Trim();
      var readings = string.Join(",", vocab.GetReadings());

      if(string.IsNullOrEmpty(readings) && KanaUtils.IsOnlyKana(question))
      {
         vocab.SetReadings([question]);
         vocab.Tags.Set(Tags.UsuallyKanaOnly);
      }

      if(vocab.CompoundParts.All().Count == 0 && vocab.PartsOfSpeech.IsSuruVerbIncluded())
      {
         var compounds = new List<string> { question.Substring(0, question.Length - 2), "する" };
         vocab.CompoundParts.SetPrimary(compounds);
      }

      if(!string.IsNullOrEmpty(vocab.GetQuestion()))
      {
         var excludedPOS = new HashSet<string> { POS.Unknown, "Godan verbIchidan verb", "Ichidan verbGodan verb" };
         var speechTypes = vocab.PartsOfSpeech.Get().Except(excludedPOS).ToHashSet();

         if(vocab.GetReadings().Any() && !speechTypes.Any())
         {
            TagAsUsuallyKanaOnlyIfTheDictionarySaysSo(vocab, _dictLookup.LookupVocabWordOrName(vocab));
            vocab.PartsOfSpeech.SetAutomaticallyFromDictionary();
         }
      }
   }

   static void TagAsUsuallyKanaOnlyIfTheDictionarySaysSo(VocabNote vocab, DictLookupResult lookup)
   {
      if(lookup.IsUk() && !vocab.Tags.Contains(Tags.DisableKanaOnly))
      {
         vocab.Tags.Set(Tags.UsuallyKanaOnly);
      }
   }
}
