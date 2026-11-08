using System.Collections.Generic;
using JAStudio.Core.Anki;
using JAStudio.Core.Note.Vocabulary;
using JAStudio.Core.Note;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class VocabDto : CorpusNoteDto
{
   public VocabQuestionDto Question { get; init; } = new();
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string UserExplanation { get; init; } = string.Empty;
   public string UserExplanationLong { get; init; } = string.Empty;
   public string UserMnemonic { get; init; } = string.Empty;
   public VocabCompoundPartsDto CompoundParts { get; init; } = new();
   public List<string> Readings { get; init; } = [];
   public string PartsOfSpeech { get; init; } = string.Empty;
   public string SourceMnemonic { get; init; } = string.Empty;
   public string SourceReadingMnemonic { get; init; } = string.Empty;
   public string AudioB { get; init; } = string.Empty;
   public string AudioG { get; init; } = string.Empty;
   public string AudioTTS { get; init; } = string.Empty;
   public List<EnteredFormDto> Forms { get; init; } = [];
   public int SentenceCount { get; init; }
   public string TechnicalNotes { get; init; } = string.Empty;
   public string References { get; init; } = string.Empty;
   public string Image { get; init; } = string.Empty;
   public string UserImage { get; init; } = string.Empty;
   public VocabMatchingRulesDto MatchingRules { get; init; } = new();
   public VocabRelatedDto RelatedVocab { get; init; } = new();

   protected override NoteId CreateTypedId() => new VocabId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
   {
      fields[AnkiFieldNames.Question] = VocabNoteQuestion.DisambiguationNameOf(Question);
      fields[AnkiFieldNames.Vocab.AudioB] = AudioB;
      fields[AnkiFieldNames.Vocab.AudioG] = AudioG;
      fields[AnkiFieldNames.Vocab.AudioTTS] = AudioTTS;
   }
}
