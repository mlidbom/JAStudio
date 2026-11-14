using JAStudio.Core.LanguageServices.JamdictEx;

namespace JAStudio.Core.Specifications.Fixtures;

/// <summary>
/// Loads JAStudio's dictionary before any specification runs (<see cref="DictLookup.LoadTheDictionary"/>), so that no specification's time includes<br/>
/// loading it, nor generating its database where there is none yet.
/// </summary>
/// <remarks>
/// The dictionary's database is generated from JMdict on first use where it doesn't exist, as on every CI runner, which starts clean: on Linux CI<br/>
/// that took about a minute. Every specification that needed the dictionary meanwhile waited for it, and one that waits 30 seconds for an import run in<br/>
/// the background, whose analysis of the imported sentence looks words up, failed: the run was executed 46 to 78 seconds late (2026-11-14).
/// </remarks>
public sealed class TheDictionaryLoadedBeforeAnySpecification
{
   public TheDictionaryLoadedBeforeAnySpecification() => DictLookup.LoadTheDictionary();
}
