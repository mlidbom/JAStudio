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

      if(vocab.CompoundParts.All().Count == 0 && IsAWordFollowedBySuru(vocab))
      {
         var compounds = new List<WordReference> { new(question.Substring(0, question.Length - 2)), new("する") };
         vocab.CompoundParts.SetPrimary(compounds);
      }
   }

   /// <summary>
   /// Whether <paramref name="vocab"/> is a word followed by する, such as 勉強する: its question ends in する, and it isn't a godan verb, such as こする,<br/>
   /// whose する is part of the verb.
   /// </summary>
   static bool IsAWordFollowedBySuru(VocabNote vocab) => vocab.PartsOfSpeech.IsSuruVerbIncluded() && !vocab.PartsOfSpeech.IsGodan();
}
