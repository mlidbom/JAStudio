using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Vocabulary.Tevents;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's yield-to-surface rule, a rule on display rather than on matching: a valid match of the vocab isn't displayed when its word's surface<br/>
/// is one of <see cref="Surfaces"/> and has a valid match of its own. So a vocab matched by a word's base form can yield to the word as it's written.
/// </summary>
/// <remarks>
/// A change publishes <see cref="IVocabTevent.MatchingConfiguration.PropertyUpdated.YieldToSurface"/>, which the matching configuration's applier<br/>
/// applies (<see cref="Apply"/>).
/// </remarks>
public class VocabYieldToSurface
{
   readonly VocabNote _vocab;

   /// <param name="vocab">The vocab it's of.</param>
   /// <param name="surfaces">The <see cref="Surfaces"/>, as the corpus stores them.</param>
   internal VocabYieldToSurface(VocabNote vocab, IEnumerable<string> surfaces)
   {
      _vocab = vocab;
      Surfaces = InTheOrderTheCorpusStoresThem(surfaces);
   }

   /// <summary>The surfaces the vocab yields to, each once, in the order the corpus stores them: ordinal, so that the same surfaces always write the same file.</summary>
   public IReadOnlyList<string> Surfaces { get; private set; }

   /// <summary>Makes the vocab yield to <paramref name="surface"/> too.</summary>
   public void Add(string surface) => SetTheSurfaces([.. Surfaces, surface]);

   /// <summary>Makes the vocab no longer yield to <paramref name="surface"/>.</summary>
   public void Remove(string surface) => SetTheSurfaces(Surfaces.Where(it => it != surface));

   /// <summary>What the rule adds to the vocab's priority while it has surfaces, as it did before match-conditions.</summary>
   internal int MatchWeight => Surfaces.Count > 0 ? 2 : 0;

   internal void OverwriteWith(VocabYieldToSurface other) => SetTheSurfaces(other.Surfaces);

   /// <summary>Makes <paramref name="surfaces"/> those the vocab yields to: what the rule's tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<string> surfaces) => Surfaces = surfaces;

   /// <summary>The surfaces as the corpus stores them.</summary>
   internal List<string> ToDto() => [.. Surfaces];

   void SetTheSurfaces(IEnumerable<string> surfaces) =>
      _vocab.PublishIfTheListChanged(Surfaces, InTheOrderTheCorpusStoresThem(surfaces), static it => new VocabTevent.MatchingConfiguration.YieldToSurfaceUpdated(it));

   static List<string> InTheOrderTheCorpusStoresThem(IEnumerable<string> surfaces) => [.. surfaces.Distinct().Order(StringComparer.Ordinal)];
}
