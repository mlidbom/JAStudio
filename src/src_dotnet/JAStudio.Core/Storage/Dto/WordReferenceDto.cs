using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="WordReference"/> as the corpus stores it, the same way as a vocab's question (<see cref="VocabQuestionDto"/>).</summary>
[MemoryPackable]
public partial class WordReferenceDto
{
   /// <summary>The <see cref="WordReference.Text"/>.</summary>
   public string Text { get; init; } = string.Empty;

   /// <summary>The <see cref="WordReference.Disambiguation"/>, empty for a reference without one.</summary>
   public string Disambiguation { get; init; } = string.Empty;
}
