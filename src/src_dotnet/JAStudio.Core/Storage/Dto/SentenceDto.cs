using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Anki;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

[MemoryPackable]
public partial class SentenceDto : CorpusNoteDto
{
   public string SourceQuestion { get; init; } = string.Empty;
   public string UserQuestion { get; init; } = string.Empty;
   public string ActiveQuestion { get; init; } = string.Empty;
   public string SourceAnswer { get; init; } = string.Empty;
   public string UserAnswer { get; init; } = string.Empty;
   public string ActiveAnswer { get; init; } = string.Empty;
   public string SourceComments { get; init; } = string.Empty;
   public string UserComments { get; init; } = string.Empty;
   public string Reading { get; init; } = string.Empty;
   public string ExternalId { get; init; } = string.Empty;
   public string Audio { get; init; } = string.Empty;
   public string Screenshot { get; init; } = string.Empty;
   public SentenceConfigDto Configuration { get; init; } = new();
   public SentenceParsingResultDto? ParsingResult { get; init; }

   /// <summary>
   /// The name of the work or deck the sentence came from, as its source gave it, such as <c>Shirokuma Cafe</c>. Empty when the source gave none.<br/>
   /// JAStudio only carries it: nothing interprets it.
   /// </summary>
   /// <remarks>
   /// Declared last on purpose: the corpus's snapshot cache (MemoryPack) reads members by their position, and old snapshots read correctly only while<br/>
   /// new members come after the existing ones.
   /// </remarks>
   public string SourceName { get; init; } = string.Empty;

   protected override NoteId CreateTypedId() => new SentenceId(Id);

   protected override void PopulateAnkiNoteFields(Dictionary<string, string> fields)
   {
      fields[AnkiFieldNames.Question] = ActiveQuestion;
      fields[AnkiFieldNames.Sentence.Audio] = Audio;
   }

   static WordExclusion FromWordExclusionDto(WordExclusionDto d) =>
      d.Index == -1 ? WordExclusion.Global(d.Word) : WordExclusion.AtIndex(d.Word, d.Index);

   static ParsedMatch FromParsedMatchDto(ParsedMatchDto d) =>
      new(d.Variant, d.StartIndex, d.IsDisplayed, d.ParsedForm, d.VocabId.HasValue ? new VocabId(d.VocabId.Value) : null);

   public static ParsingResult CreateParsingResult(SentenceParsingResultDto? data)
   {
      if(data == null) return new ParsingResult([], "", "");
      return new ParsingResult(
         [.. data.ParsedWords.Select(FromParsedMatchDto)],
         data.Sentence,
         data.ParserVersion);
   }

   public static SentenceConfiguration CreateConfiguration(SentenceConfigDto? data, Action saveCallback)
   {
      if(data == null)
         return new SentenceConfiguration(
            [],
            WordExclusionSet.Empty(saveCallback),
            WordExclusionSet.Empty(saveCallback));
      return new SentenceConfiguration(
         [.. data.HighlightedWords],
         new WordExclusionSet(saveCallback, [.. data.IncorrectMatches.Select(FromWordExclusionDto)]),
         new WordExclusionSet(saveCallback, [.. data.HiddenMatches.Select(FromWordExclusionDto)]));
   }
}
