using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// The radicals a kanji is made of (<see cref="KanjiNote.Radicals"/>), each named by its character, in their order: a radical appears as often as the kanji<br/>
/// has it, as 二 is made of 一 and 一. Every change saves the kanji, through its <see cref="NoteGuard"/>.
/// </summary>
/// <remarks>
/// A kanji is never among its own radicals: adding its own character does nothing. Which kanji have a radical is the collection's index of them<br/>
/// (<see cref="KanjiCollection.WithRadical"/>), not stored on the radical.
/// </remarks>
public class KanjiRadicals
{
   readonly KanjiNote _kanji;
   readonly List<string> _radicals;
   readonly NoteGuard _guard;

   /// <param name="kanji">The kanji the radicals are of.</param>
   /// <param name="radicals">The radicals as the kanji stores them.</param>
   /// <param name="guard">Saves the kanji when its radicals change.</param>
   public KanjiRadicals(KanjiNote kanji, IEnumerable<string> radicals, NoteGuard guard)
   {
      _kanji = kanji;
      _radicals = [.. radicals];
      _guard = guard;
   }

   /// <summary>The radicals, in their order, each as often as the kanji has it.</summary>
   public IReadOnlyList<string> All => _radicals;

   public bool Contains(string radical) => _radicals.Contains(radical);

   /// <summary>The notes of the radicals that the collection has, in the radicals' order, each as often as the kanji has it.</summary>
   public List<KanjiNote> Notes() => [.. _radicals.Select(_kanji.Services.Collection.Kanji.WithKanji).OfType<KanjiNote>()];

   /// <summary>Adds <paramref name="radical"/> after the others, even when the kanji has it already, unless it's the kanji itself.</summary>
   public void Add(string radical)
   {
      if(_kanji.IsNamedBy(radical)) return;
      _guard.Update(() => _radicals.Add(radical));
   }

   /// <summary>Removes <paramref name="radical"/> once: a radical the kanji has twice is then had once.</summary>
   public void Remove(string radical)
   {
      if(!Contains(radical)) return;
      _guard.Update(() => _radicals.Remove(radical));
   }

   /// <summary>Replaces the radicals with <paramref name="radicals"/>, in their order, leaving out the kanji itself.</summary>
   public void Set(IEnumerable<string> radicals) => _guard.Update(() =>
   {
      _radicals.Clear();
      _radicals.AddRange(radicals.Where(it => !_kanji.IsNamedBy(it)));
   });

   public override string ToString() => string.Join(", ", _radicals);
}
