using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.LanguageServices.TokenizerDictionaries.Comparing;

/// <summary>How many times each of some things was found, with a few different examples of each, such as the sentences a difference was found in.</summary>
public sealed partial class Tally<TThing> where TThing : notnull
{
   /// <summary>The most examples kept of each thing.</summary>
   public const int ExamplesKept = 3;

   readonly Dictionary<TThing, Counted> _countedByThing = [];

   /// <summary>How many times anything was found.</summary>
   public long Total => _countedByThing.Values.Sum(it => (long)it.Count);

   /// <summary>Counts <paramref name="thing"/> found once more, keeping <paramref name="example"/> when it has fewer than <see cref="ExamplesKept"/> other examples.</summary>
   public void Add(TThing thing, string example) => CountedOf(thing).Add(1, [example]);

   /// <summary>Adds everything <paramref name="other"/> counted to this tally.</summary>
   public void AddEverythingIn(Tally<TThing> other)
   {
      foreach(var counted in other._countedByThing.Values) CountedOf(counted.Thing).Add(counted.Count, counted.Examples);
   }

   /// <summary>Each thing counted, the one found the most times first.</summary>
   public IReadOnlyList<Counted> MostFoundFirst() => [.. _countedByThing.Values.OrderByDescending(it => it.Count)];

   Counted CountedOf(TThing thing)
   {
      if(!_countedByThing.TryGetValue(thing, out var counted)) _countedByThing[thing] = counted = new Counted(thing);
      return counted;
   }
}
