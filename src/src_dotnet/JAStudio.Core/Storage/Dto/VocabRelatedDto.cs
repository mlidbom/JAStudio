using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's related vocab (<see cref="Note.Vocabulary.RelatedVocab.RelatedVocab"/>) as the corpus stores them.</summary>
[MemoryPackable]
public partial class VocabRelatedDto
{
   public List<string> ErgativeTwins { get; init; } = [];
   public string DerivedFrom { get; init; } = string.Empty;
   public List<string> PerfectSynonyms { get; init; } = [];
   public List<string> Synonyms { get; init; } = [];
   public List<string> Antonyms { get; init; } = [];
   public List<string> ConfusedWith { get; init; } = [];
   public List<string> SeeAlso { get; init; } = [];
}
