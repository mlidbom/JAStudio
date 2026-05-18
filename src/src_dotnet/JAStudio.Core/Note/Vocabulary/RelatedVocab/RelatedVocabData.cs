using System.Collections.Generic;
using Compze.Internals.SystemCE;
using JAStudio.Core.Note.NoteFields;
using JAStudio.Core.Note.NoteFields.AutoSaveWrappers;

namespace JAStudio.Core.Note.Vocabulary.RelatedVocab;

public class RelatedVocabData
{
   static readonly LazyCE<IObjectSerializer<RelatedVocabData>> SerializerInstance = new(() => new RelatedVocabDataSerializer());

   public static IObjectSerializer<RelatedVocabData> Serializer() => SerializerInstance.Value;

   public string ErgativeTwin { get; set; }
   public ValueWrapper<string> DerivedFrom { get; set; }
   public HashSet<string> Synonyms { get; set; }
   public HashSet<string> PerfectSynonyms { get; set; }
   public HashSet<string> Antonyms { get; set; }
   public HashSet<string> ConfusedWith { get; set; }
   public HashSet<string> SeeAlso { get; set; }

   public RelatedVocabData(
      string ergativeTwin,
      ValueWrapper<string> derivedFrom,
      HashSet<string> perfectSynonyms,
      HashSet<string> similar,
      HashSet<string> antonyms,
      HashSet<string> confusedWith,
      HashSet<string> seeAlso)
   {
      ErgativeTwin = ergativeTwin;
      DerivedFrom = derivedFrom;
      Synonyms = similar;
      PerfectSynonyms = perfectSynonyms;
      Antonyms = antonyms;
      ConfusedWith = confusedWith;
      SeeAlso = seeAlso;
   }

   public override string ToString() =>
      $"RelatedVocabData(ergative_twin={ErgativeTwin}, derived_from={DerivedFrom}, " +
      $"perfect_synonyms={string.Join(",", PerfectSynonyms)}, synonyms={string.Join(",", Synonyms)}, " +
      $"antonyms={string.Join(",", Antonyms)}, confused_with={string.Join(",", ConfusedWith)}, " +
      $"see_also={string.Join(",", SeeAlso)})";
}
