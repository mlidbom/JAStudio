using System.Collections.Generic;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="Note.Vocabulary.VocabNote"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class VocabDto : CorpusNoteDto
{
   public VocabQuestionDto Question { get; init; } = new();
   public string CorpusAnswer { get; init; } = string.Empty;
   public string Explanation { get; init; } = string.Empty;
   public string ExplanationLong { get; init; } = string.Empty;
   public string Mnemonic { get; init; } = string.Empty;
   public VocabCompoundPartsDto CompoundParts { get; init; } = new();
   public List<string> Readings { get; init; } = [];
   public List<string> PartsOfSpeech { get; init; } = [];
   /// <summary>The names of the vocab's <see cref="Note.Vocabulary.VocabNoteRegister"/> flags that are set.</summary>
   public List<string> Register { get; init; } = [];
   /// <summary>The vocab's <see cref="Note.Vocabulary.VocabNoteUsuallyWrittenInKana.Decision"/>: true for yes, false for no, and null while it's undecided.</summary>
   public bool? UsuallyWrittenInKana { get; init; }
   public string AudioB { get; init; } = string.Empty;
   public string AudioG { get; init; } = string.Empty;
   public string AudioTTS { get; init; } = string.Empty;
   public List<EnteredFormDto> Forms { get; init; } = [];
   public int SentenceCount { get; init; }
   public string TechnicalNotes { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string Image { get; init; } = string.Empty;
   public string UserImage { get; init; } = string.Empty;
   public VocabMatchingDto Matching { get; init; } = new();
   public VocabRelatedDto RelatedVocab { get; init; } = new();
   public VocabSourceDataDto SourceData { get; init; } = new();
}
