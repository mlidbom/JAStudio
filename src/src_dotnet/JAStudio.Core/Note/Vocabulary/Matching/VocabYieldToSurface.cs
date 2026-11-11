using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's yield-to-surface rule, a rule on display rather than on matching: a valid match of the vocab isn't displayed when its word's surface<br/>
/// is one of <see cref="Surfaces"/> and has a valid match of its own. So a vocab matched by a word's base form can yield to the word as it's written.
/// </summary>
public class VocabYieldToSurface
{
   readonly NoteGuard _guard;
   readonly HashSet<string> _surfaces;

   /// <param name="surfaces">The <see cref="Surfaces"/>, as the corpus stores them.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabYieldToSurface(IEnumerable<string> surfaces, NoteGuard guard)
   {
      _guard = guard;
      _surfaces = [.. surfaces];
   }

   /// <summary>The surfaces the vocab yields to.</summary>
   public IReadOnlySet<string> Surfaces => _surfaces;

   /// <summary>Makes the vocab yield to <paramref name="surface"/> too, and saves the vocab.</summary>
   public void Add(string surface) => _guard.Update(() => _surfaces.Add(surface));

   /// <summary>Makes the vocab no longer yield to <paramref name="surface"/>, and saves the vocab.</summary>
   public void Remove(string surface) => _guard.Update(() => _surfaces.Remove(surface));

   /// <summary>What the rule adds to the vocab's priority while it has surfaces, as it did before match-conditions.</summary>
   internal int MatchWeight => _surfaces.Count > 0 ? 2 : 0;

   internal void OverwriteWith(VocabYieldToSurface other) =>
      _guard.Update(() =>
      {
         _surfaces.Clear();
         _surfaces.UnionWith(other._surfaces);
      });

   /// <summary>The surfaces as the corpus stores them: in ordinal order, so that the same surfaces always write the same file.</summary>
   internal List<string> ToDto() => [.. _surfaces.Order(StringComparer.Ordinal)];
}
