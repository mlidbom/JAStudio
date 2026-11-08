using System;
using System.Linq;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="Note.Sentences.SentenceNote"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class SentenceDto : CorpusNoteDto
{
   public string CorpusQuestion { get; init; } = string.Empty;
   public string CorpusAnswer { get; init; } = string.Empty;
   public string Comments { get; init; } = string.Empty;
   public string Reading { get; init; } = string.Empty;
   public string Audio { get; init; } = string.Empty;
   public string Screenshot { get; init; } = string.Empty;
   public SentenceConfigDto Configuration { get; init; } = new();
   public SentenceParsingResultDto? ParsingResult { get; init; }

   /// <summary>
   /// The name of the work or deck the sentence came from, as its source gave it, such as <c>Shirokuma Cafe</c>. Empty when the source gave none.<br/>
   /// JAStudio only carries it: nothing interprets it.
   /// </summary>
   public string SourceName { get; init; } = string.Empty;

   public SentenceSourceDataDto SourceData { get; init; } = new();

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
