using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Sentences.ParsingCases.Breakdowns;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A <see cref="BreakdownUnderOptions"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class BreakdownUnderOptionsDto
{
   /// <summary>The value of each option that changes the breakdown, by its <see cref="BreakdownParsingOption.Name"/>. Empty when none does.</summary>
   public Dictionary<string, bool> Options { get; init; } = new();

   public List<BreakdownWordDto> DisplayedWords { get; init; } = [];

   public static BreakdownUnderOptionsDto From(BreakdownUnderOptions breakdown) =>
      new()
      {
         Options = BreakdownParsingOption.All.Where(it => breakdown.Options.ContainsKey(it.Name)).ToDictionary(it => it.Name, it => breakdown.Options[it.Name]),
         DisplayedWords = [.. breakdown.DisplayedWords.Select(BreakdownWordDto.From)]
      };

   public BreakdownUnderOptions ToBreakdownUnderOptions() => new(Options, [.. DisplayedWords.Select(it => it.ToBreakdownWord())]);
}
