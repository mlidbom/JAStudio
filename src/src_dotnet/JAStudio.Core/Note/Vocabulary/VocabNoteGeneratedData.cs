using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
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
         vocab.CompoundParts.Set(compounds);
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

   /// <summary>
   /// Sets up the forms of a vocab just added in Anki, as Yomitan adds them through AnkiConnect: takes them from the dictionary,<br/>
   /// and tags the vocab usually kana only if the dictionary says so. Only a vocab with a reading can be looked up.
   /// </summary>
   /// <remarks>
   /// A vocab added in Anki brings no forms and no readings, since only the fields <see cref="AnkiVocabNote"/> exposes are taken from Anki.<br/>
   /// So this is called after <see cref="UpdateGeneratedData"/>, which takes the reading of a vocab whose question is in kana from its question.
   /// </remarks>
   /// <remarks>
   /// Only a vocab just added in Anki is known to be new. Most vocab have no forms besides their question,<br/>
   /// so having none can't be taken as a sign that a vocab was never set up.
   /// </remarks>
   public void SetUpTheFormsOfVocabAddedInAnki(VocabNote vocab)
   {
      if(!vocab.GetReadings().Any()) return;

      var lookup = _dictLookup.LookupVocabWordOrName(vocab);
      TagAsUsuallyKanaOnlyIfTheDictionarySaysSo(vocab, lookup);

      if(lookup.FoundWords())
      {
         vocab.Forms.SetEnteredForms(lookup.ValidForms(vocab.PartsOfSpeech.IsUk()));
      }

      if(vocab.PartsOfSpeech.IsUk() && !vocab.Forms.QuestionAndEnteredFormsList().Contains(vocab.GetReadings()[0]))
      {
         vocab.Forms.SetEnteredForms(vocab.Forms.EnteredFormsSet().Union(vocab.GetReadings()));
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
