using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A vocab's <see cref="VocabNoteCompoundParts"/> as the corpus stores them.</summary>
[MemoryPackable]
public partial class VocabCompoundPartsDto
{
   /// <summary>The <see cref="VocabNoteCompoundParts.Primary"/> parts, in their order.</summary>
   public List<WordReferenceDto> Primary { get; init; } = [];

   /// <summary>
   /// The <see cref="VocabNoteCompoundParts.EnteredAlternatives"/> parts, those also primary included: a set, written in a fixed order so that the same<br/>
   /// set always writes the same file.
   /// </summary>
   public List<WordReferenceDto> Alternative { get; init; } = [];
}
