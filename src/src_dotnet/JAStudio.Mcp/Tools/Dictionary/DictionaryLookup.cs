using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;

namespace JAStudio.Mcp.Tools.Dictionary;

/// <summary>What <see cref="DictionaryTools.LookUpInTheDictionary"/> reports: the dictionary's entries found, with the parts of speech JAStudio maps them to.</summary>
/// <param name="PartsOfSpeech">The parts of speech of all the entries together, in alphabetical order: what a vocab looked up gets from "Repopulate parts of speech".</param>
public record DictionaryLookup(IReadOnlyList<string> PartsOfSpeech, IReadOnlyList<DictionaryEntryFound> Entries)
{
   public static DictionaryLookup Of(DictLookupResult lookup) =>
      new([.. lookup.PartsOfSpeech().Order().Select(it => it.Name)], [.. lookup.Entries.Select(DictionaryEntryFound.Of)]);
}
