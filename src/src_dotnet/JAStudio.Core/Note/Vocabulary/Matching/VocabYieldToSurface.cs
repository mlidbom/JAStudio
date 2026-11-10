using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.Vocabulary.Matching;

/// <summary>
/// A vocab's yield-to-surface rule, a rule on display rather than on matching: a valid match of the vocab isn't displayed when its word's surface<br/>
/// is one of <see cref="Surfaces"/> and has a valid match of its own. So a vocab matched by a word's base form can yield to the word as it's written.
/// </summary>
/// <remarks>Changing <see cref="Surfaces"/> saves nothing by itself: <see cref="Save"/> saves the note.</remarks>
public class VocabYieldToSurface
{
   readonly NoteGuard _guard;

   /// <param name="surfaces">The <see cref="Surfaces"/>, as the corpus stores them.</param>
   /// <param name="guard">Saves the vocab after a change.</param>
   internal VocabYieldToSurface(IEnumerable<string> surfaces, NoteGuard guard)
   {
      _guard = guard;
      Surfaces = [.. surfaces];
   }

   /// <summary>The surfaces the vocab yields to.</summary>
   public HashSet<string> Surfaces { get; }

   /// <summary>Saves the vocab, after a change to <see cref="Surfaces"/>.</summary>
   public void Save() => _guard.Update(() => {});

   /// <summary>What the rule adds to the vocab's priority while it has surfaces, as it did before match-conditions.</summary>
   internal int MatchWeight => Surfaces.Count > 0 ? 2 : 0;

   internal void OverwriteWith(VocabYieldToSurface other)
   {
      Surfaces.Clear();
      Surfaces.UnionWith(other.Surfaces);
      Save();
   }

   /// <summary>The surfaces as the corpus stores them: in ordinal order, so that the same surfaces always write the same file.</summary>
   internal List<string> ToDto() => [.. Surfaces.Order(StringComparer.Ordinal)];
}
