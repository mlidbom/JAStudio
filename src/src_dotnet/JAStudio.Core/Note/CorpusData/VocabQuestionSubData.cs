using JAStudio.Core.Note.Vocabulary;
using MemoryPack;

namespace JAStudio.Core.Note.CorpusData;

/// <summary>A vocab's <see cref="VocabNoteQuestion"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class VocabQuestionSubData
{
   /// <summary>The <see cref="VocabNoteQuestion.Text"/>.</summary>
   public string Text { get; init; } = string.Empty;

   /// <summary>The <see cref="VocabNoteQuestion.Disambiguation"/>, empty for a vocab without one.</summary>
   public string Disambiguation { get; init; } = string.Empty;
}
