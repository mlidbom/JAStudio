using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using JAStudio.Core.LanguageServices.JanomeEx.WordExtraction;

namespace JAStudio.Core.Specifications.LanguageServices.ParsingCases.Breakdowns;

/// <summary>
/// One of the <see cref="BreakdownParsingOptions"/>, by name, so that a parsing case can name the options that change its sentence breakdown,<br/>
/// and every combination of them can be made.
/// </summary>
/// <param name="Name">The option's name as a case file writes it: its <see cref="BreakdownParsingOptions"/> property's, in camel case.</param>
public sealed record BreakdownParsingOption(string Name, Func<BreakdownParsingOptions, bool> IsOnIn, Func<BreakdownParsingOptions, bool, BreakdownParsingOptions> With)
{
   /// <summary>Every one of the <see cref="BreakdownParsingOptions"/>, in their order.</summary>
   public static IReadOnlyList<BreakdownParsingOption> All { get; } =
   [
      Named(nameof(BreakdownParsingOptions.HideAllCompounds), it => it.HideAllCompounds, (it, on) => it with { HideAllCompounds = on }),
      Named(nameof(BreakdownParsingOptions.HideCompositionallyTransparentCompounds), it => it.HideCompositionallyTransparentCompounds, (it, on) => it with { HideCompositionallyTransparentCompounds = on }),
      Named(nameof(BreakdownParsingOptions.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound),
            it => it.AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound,
            (it, on) => it with { AutomaticallyYieldLastTokenInSuruVerbCompoundsToOverlappingCompound = on }),
      Named(nameof(BreakdownParsingOptions.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound),
            it => it.AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound,
            (it, on) => it with { AutomaticallyYieldLastTokenInPassiveVerbCompoundsToOverlappingCompound = on }),
      Named(nameof(BreakdownParsingOptions.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound),
            it => it.AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound,
            (it, on) => it with { AutomaticallyYieldLastTokenInCausativeVerbCompoundsToOverlappingCompound = on })
   ];

   /// <summary>Every combination of <paramref name="options"/> being on and off, with every other option off: two to the power of their count.</summary>
   public static IReadOnlyList<BreakdownParsingOptions> EveryCombinationOf(IEnumerable<BreakdownParsingOption> options) =>
      options.Aggregate<BreakdownParsingOption, IReadOnlyList<BreakdownParsingOptions>>([new BreakdownParsingOptions(false, false, false, false, false)],
                                                                                   (combinations, option) => [.. combinations.SelectMany(it => new[] { option.With(it, false), option.With(it, true) })]);

   /// <summary><paramref name="options"/> with this option switched to the other value.</summary>
   public BreakdownParsingOptions SwitchedIn(BreakdownParsingOptions options) => With(options, !IsOnIn(options));

   static BreakdownParsingOption Named(string propertyName,
                                      Func<BreakdownParsingOptions, bool> isOnIn,
                                      Func<BreakdownParsingOptions, bool, BreakdownParsingOptions> with) =>
      new(JsonNamingPolicy.CamelCase.ConvertName(propertyName), isOnIn, with);
}
