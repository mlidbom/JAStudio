using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.CorpusData;
using JAStudio.Core.Note.Tagging;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNotePartsOfSpeech
{
   readonly NoteGuard _guard;
   string _value;

   public VocabNotePartsOfSpeech(VocabNote vocab, VocabData? data, NoteGuard guard)
   {
      Vocab = vocab;
      _guard = guard;
      _value = POSSetManager.InternAndHarmonize(data?.PartsOfSpeech ?? string.Empty);
   }

   VocabNote Vocab { get; }

   public string RawStringValue() => _value;

   public void SetRawStringValue(string value)
   {
      _guard.Update(() => _value = POSSetManager.InternAndHarmonize(value));
   }

   public void Set(IEnumerable<string> value)
   {
      SetRawStringValue(string.Join(",", value));
   }

   public HashSet<string> Get() => POSSetManager.Get(RawStringValue()).ToHashSet();

   public bool IsIchidan() => Get().Contains(POS.IchidanVerb);
   public bool IsGodan() => Get().Contains(POS.GodanVerb);
   public bool IsTransitive() => Get().Contains(POS.Transitive);
   public bool IsIntransitive() => Get().Contains(POS.Intransitive);
   public bool IsInflectingWordType() => IsGodan() || IsIchidan();

   public bool IsSuruVerbIncluded()
   {
      var question = Vocab.Question.WithoutNoiseCharacters;
      return question.Length > 2 && question.EndsWith("する");
   }

   static readonly HashSet<string> GaSuruNiSuruEndings = ["がする", "にする", "くする"];

   public bool IsNiSuruGaSuruKuSuruCompound()
   {
      var question = Vocab.Question.WithoutNoiseCharacters;
      if(question.Length <= 3)
         return false;

      var ending = question.Substring(question.Length - 3);
      return GaSuruNiSuruEndings.Contains(ending);
   }

   public bool IsUk() => Vocab.Tags.Contains(Tags.UsuallyKanaOnly);

   public void SetAutomaticallyFromDictionary()
   {
      var lookup = Vocab.Services.DictLookup.LookupVocabWordOrName(Vocab);
      if(lookup.FoundWords())
      {
         var value = string.Join(", ", lookup.PartsOfSpeech());
         SetRawStringValue(value);
      } else if(IsSuruVerbIncluded())
      {
         var question = Vocab.Question.WithoutNoiseCharacters;
         question = question.Substring(0, question.Length - 2);
         var readings = Vocab.GetReadings().Select(r => r.Substring(0, r.Length - 2)).ToList();
         lookup = Vocab.Services.DictLookup.LookupWordOrNameWithMatchingReading(question, readings);
         var pos = lookup.PartsOfSpeech().Intersect([POS.Transitive, POS.Intransitive]).ToList();
         var value = POS.SuruVerb + ", " + string.Join(", ", pos);
         SetRawStringValue(value);
      }
   }

   public bool IsPassiveVerbCompound()
   {
      var compounds = Vocab.CompoundParts.Primary();
      if(compounds.Count == 0) return false;
      return AnalysisConstants.PassiveVerbEndings.Contains(compounds[compounds.Count - 1]);
   }

   public bool IsCausativeVerbCompound()
   {
      var compounds = Vocab.CompoundParts.Primary();
      if(compounds.Count == 0) return false;
      return AnalysisConstants.CausativeVerbEndings.Contains(compounds[compounds.Count - 1]);
   }

   public bool IsCompleteNaAdjective() => Vocab.Question.Raw.EndsWith("な") && Get().Contains(POS.NaAdjective);

   public override string ToString() => RawStringValue();
}
