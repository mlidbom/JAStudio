using System.Collections.Generic;
using JAStudio.Dictionary;

namespace JAStudio.Mcp.Tools.Dictionary;

/// <summary>One of the senses of a dictionary entry, as the dictionary has it, in <see cref="DictionaryTools.LookUpInTheDictionary"/>'s report.</summary>
/// <param name="PartsOfSpeech">The dictionary's own parts of speech, as it describes them, such as "godan verb with 'ku' ending".</param>
/// <param name="Miscellanea">The dictionary's other notes on the sense, such as "word usually written using kana alone".</param>
public record DictionarySense(IReadOnlyList<string> Glosses, IReadOnlyList<string> PartsOfSpeech, IReadOnlyList<string> Miscellanea)
{
   public static DictionarySense Of(WordSense sense) => new(sense.Glosses, sense.PartsOfSpeech, sense.Miscellanea);
}
