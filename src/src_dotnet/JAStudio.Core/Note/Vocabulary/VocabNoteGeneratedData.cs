using System.Collections.Generic;
using JAStudio.Core.LanguageServices;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteGeneratedData
{
   internal VocabNoteGeneratedData() {}

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
   }
}
