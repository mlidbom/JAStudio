using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>An <see cref="EnteredForm"/> as the corpus stores it, in a vocab's <see cref="VocabDto.Forms"/>.</summary>
[MemoryPackable]
public partial class EnteredFormDto
{
   public string Text { get; init; } = string.Empty;
   public bool IsOwned { get; init; }
}
