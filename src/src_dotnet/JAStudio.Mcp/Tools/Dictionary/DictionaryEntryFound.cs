using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.LanguageServices.JamdictEx;

namespace JAStudio.Mcp.Tools.Dictionary;

/// <summary>One of the dictionary's entries a lookup found, as <see cref="DictionaryTools.LookUpInTheDictionary"/> reports it.</summary>
/// <param name="PartsOfSpeech">The parts of speech JAStudio maps the entry's senses' parts of speech to, in alphabetical order: the ones a vocab can have.</param>
public record DictionaryEntryFound(IReadOnlyList<string> KanjiForms, IReadOnlyList<string> KanaForms, IReadOnlyList<string> PartsOfSpeech, IReadOnlyList<DictionarySense> Senses)
{
   public static DictionaryEntryFound Of(DictEntry entry) =>
      new(entry.KanjiFormsText(),
          entry.KanaFormsText(),
          [.. entry.PartsOfSpeech().Order().Select(it => it.Name)],
          [.. entry.Senses.Select(DictionarySense.Of)]);
}
