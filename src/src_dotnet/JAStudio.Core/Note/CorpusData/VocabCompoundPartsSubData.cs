using System.Collections.Generic;
using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

/// <summary>A vocab's <see cref="VocabNoteCompoundParts"/> as the corpus stores them.</summary>
[MemoryPackable]
public partial class VocabCompoundPartsSubData
{
   /// <summary>The <see cref="VocabNoteCompoundParts.Primary"/> parts, in their order.</summary>
   public List<string> Primary { get; init; } = [];

   /// <summary>The <see cref="VocabNoteCompoundParts.Alternative"/> parts: a set, written in a fixed order so that the same set always writes the same file.</summary>
   public List<string> Alternative { get; init; } = [];
}
