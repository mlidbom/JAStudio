using System;
using JAStudio.Core.Note.Sentences.ParsingCases;
using MemoryPack;

namespace JAStudio.Core.Storage.Dto;

/// <summary>A sentence's <see cref="ParsingCase"/> as the corpus stores it.</summary>
[MemoryPackable]
public partial class ParsingCaseDto
{
   const string Unverified = "unverified";
   const string Verified = "verified";
   const string Broken = "broken";

   public string Name { get; init; } = string.Empty;
   public string Why { get; init; } = string.Empty;

   /// <summary>The <see cref="ParsingCase.VerdictOnTheDisplayedVocab"/>: <c>unverified</c>, <c>verified</c> or <c>broken</c>.</summary>
   public string VerdictOnTheDisplayedVocab { get; init; } = Unverified;

   public ParsingCaseResultDto Expected { get; init; } = new();

   public static ParsingCaseDto From(ParsingCase parsingCase) =>
      new()
      {
         Name = parsingCase.Name,
         Why = parsingCase.Why,
         VerdictOnTheDisplayedVocab = parsingCase.VerdictOnTheDisplayedVocab switch
         {
            Verdict.Unverified => Unverified,
            Verdict.Verified => Verified,
            Verdict.Broken => Broken,
            _ => throw new ArgumentOutOfRangeException(nameof(parsingCase), parsingCase.VerdictOnTheDisplayedVocab, "A verdict the corpus doesn't store.")
         },
         Expected = ParsingCaseResultDto.From(parsingCase.Expected)
      };

   public ParsingCase ToParsingCase() =>
      new(Name,
          Why,
          VerdictOnTheDisplayedVocab switch
          {
             Unverified => Verdict.Unverified,
             Verified => Verdict.Verified,
             Broken => Verdict.Broken,
             _ => throw new InvalidOperationException($"The parsing case {Name} has the verdict \"{VerdictOnTheDisplayedVocab}\": a verdict is \"{Unverified}\", \"{Verified}\" or \"{Broken}\".")
          },
          Expected.ToParsingCaseResult());
}
