using System.Collections.Generic;
using JAStudio.Core.Note.NoteFields.AutoSaveWrappers;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class RelatedVocabData
{
   public HashSet<string> ErgativeTwins { get; set; }
   public ValueWrapper<string> DerivedFrom { get; set; }
   public HashSet<string> Synonyms { get; set; }
   public HashSet<string> PerfectSynonyms { get; set; }
   public HashSet<string> Antonyms { get; set; }
   public HashSet<string> ConfusedWith { get; set; }
   public HashSet<string> SeeAlso { get; set; }

   public RelatedVocabData(
      HashSet<string> ergativeTwins,
      ValueWrapper<string> derivedFrom,
      HashSet<string> perfectSynonyms,
      HashSet<string> similar,
      HashSet<string> antonyms,
      HashSet<string> confusedWith,
      HashSet<string> seeAlso)
   {
      ErgativeTwins = ergativeTwins;
      DerivedFrom = derivedFrom;
      Synonyms = similar;
      PerfectSynonyms = perfectSynonyms;
      Antonyms = antonyms;
      ConfusedWith = confusedWith;
      SeeAlso = seeAlso;
   }

   public override string ToString() =>
      $"RelatedVocabData(ergative_twins={string.Join(",", ErgativeTwins)}, derived_from={DerivedFrom}, " +
      $"perfect_synonyms={string.Join(",", PerfectSynonyms)}, synonyms={string.Join(",", Synonyms)}, " +
      $"antonyms={string.Join(",", Antonyms)}, confused_with={string.Join(",", ConfusedWith)}, " +
      $"see_also={string.Join(",", SeeAlso)})";
}
