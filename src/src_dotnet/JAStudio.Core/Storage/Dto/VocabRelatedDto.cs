using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's related vocab (<see cref="Note.Vocabulary.RelatedVocab.RelatedVocab"/>) as the corpus stores them: word-references, each in a fixed order.</summary>
[MemoryPackable]
public partial class VocabRelatedDto
{
   public List<WordReferenceDto> ErgativeTwins { get; init; } = [];

   /// <summary>The word the vocab is derived from, or null when none is given.</summary>
   public WordReferenceDto? DerivedFrom { get; init; }

   public List<WordReferenceDto> PerfectSynonyms { get; init; } = [];
   public List<WordReferenceDto> Synonyms { get; init; } = [];
   public List<WordReferenceDto> Antonyms { get; init; } = [];
   public List<WordReferenceDto> ConfusedWith { get; init; } = [];
   public List<WordReferenceDto> SeeAlso { get; init; } = [];
}
