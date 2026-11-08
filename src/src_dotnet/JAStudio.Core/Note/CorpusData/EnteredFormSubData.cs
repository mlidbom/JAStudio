using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

/// <summary>An <see cref="EnteredForm"/> as the corpus stores it, in a vocab's <see cref="VocabData.Forms"/>.</summary>
[MemoryPackable]
public partial class EnteredFormSubData
{
   public string Text { get; init; } = string.Empty;
   public bool IsOwned { get; init; }
}
