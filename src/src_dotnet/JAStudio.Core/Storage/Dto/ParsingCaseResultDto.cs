using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Sentences.ParsingCases;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="ParsingCaseResult"/> as the corpus stores it: with each vocab by its id alone, as a sentence's parsing result is stored.</summary>
[MemoryPackable]
public partial class ParsingCaseResultDto
{
   public List<ParsedMatchDto> ParsedWords { get; init; } = [];
   public List<BreakdownUnderOptionsDto> Breakdowns { get; init; } = [];

   public static ParsingCaseResultDto From(ParsingCaseResult result) =>
      new()
      {
         ParsedWords = [.. result.ParsedWords.Select(it => it.ToParsedMatch()).Select(ToParsedMatchDto)],
         Breakdowns = [.. result.Breakdowns.Select(BreakdownUnderOptionsDto.From)]
      };

   public ParsingCaseResult ToParsingCaseResult() =>
      new([.. ParsedWords.Select(ToParsedMatch).Select(ParsedWordReported.NotNamingItsVocab)],
          [.. Breakdowns.Select(it => it.ToBreakdownUnderOptions())]);

   static ParsedMatchDto ToParsedMatchDto(ParsedMatch match) =>
      new()
      {
         Variant = match.Variant,
         StartIndex = match.StartIndex,
         IsDisplayed = match.IsDisplayed,
         ParsedForm = match.ParsedForm,
         VocabId = match.VocabId?.Value
      };

   static ParsedMatch ToParsedMatch(ParsedMatchDto match) =>
      new(match.Variant, match.StartIndex, match.IsDisplayed, match.ParsedForm, match.VocabId.HasValue ? VocabId.Shared(match.VocabId.Value) : null);
}
