using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices;
using JAStudio.Core.LanguageServices.JamdictEx;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteGeneratedData
{
   readonly DictLookup _dictLookup;
   internal VocabNoteGeneratedData(DictLookup dictLookup) => _dictLookup = dictLookup;

   public void UpdateGeneratedData(VocabNote vocab)
   {
      vocab.MetaData.SetSentenceCount(vocab.Sentences.All().Count);
      vocab.RelatedNotes.PerfectSynonyms.PushAnswerToOtherSynonyms();

      var question = vocab.Question.WithoutNoiseCharacters.Trim();
      var readings = string.Join(",", vocab.GetReadings());

      if(string.IsNullOrEmpty(readings) && KanaUtils.IsOnlyKana(question))
      {
         vocab.SetReadings([question]);
         vocab.UsuallyWrittenInKana.DecideAsYesUnlessDecided();
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
            if(_dictLookup.LookupVocabWordOrName(vocab).IsUk()) vocab.UsuallyWrittenInKana.DecideAsYesUnlessDecided();
            vocab.PartsOfSpeech.SetAutomaticallyFromDictionary();
         }
      }
   }
}
