using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.KanjiTevents;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// The radicals a kanji is made of (<see cref="KanjiNote.Radicals"/>), each named by its character, in their order: a radical appears as often as the kanji<br/>
/// has it, as 二 is made of 一 and 一. A change publishes <see cref="IKanjiTevent.PropertyUpdated.Radicals"/>, the whole new list, which the kanji applies.
/// </summary>
/// <remarks>
/// A kanji is never among its own radicals: adding its own character does nothing. Which kanji have a radical is the collection's index of them<br/>
/// (<see cref="KanjiCollection.WithRadical"/>), not stored on the radical.
/// </remarks>
public class KanjiRadicals
{
   readonly KanjiNote _kanji;

   /// <param name="kanji">The kanji the radicals are of.</param>
   /// <param name="radicals">The radicals as the kanji stores them.</param>
   public KanjiRadicals(KanjiNote kanji, IEnumerable<string> radicals)
   {
      _kanji = kanji;
      All = [.. radicals];
   }

   /// <summary>The radicals, in their order, each as often as the kanji has it.</summary>
   public IReadOnlyList<string> All { get; private set; }

   public bool Contains(string radical) => All.Contains(radical);

   /// <summary>The notes of the radicals that the collection has, in the radicals' order, each as often as the kanji has it.</summary>
   public List<KanjiNote> Notes() => [.. All.Select(_kanji.Services.Collection.Kanji.WithKanji).OfType<KanjiNote>()];

   /// <summary>Adds <paramref name="radical"/> after the others, even when the kanji has it already, unless it's the kanji itself.</summary>
   public void Add(string radical)
   {
      if(_kanji.IsNamedBy(radical)) return;
      SetTheRadicals([.. All, radical]);
   }

   /// <summary>Removes <paramref name="radical"/> once: a radical the kanji has twice is then had once.</summary>
   public void Remove(string radical)
   {
      List<string> radicals = [.. All];
      radicals.Remove(radical);
      SetTheRadicals(radicals);
   }

   /// <summary>Replaces the radicals with <paramref name="radicals"/>, in their order, leaving out the kanji itself.</summary>
   public void Set(IEnumerable<string> radicals) => SetTheRadicals([.. radicals.Where(it => !_kanji.IsNamedBy(it))]);

   /// <summary>Makes <paramref name="radicals"/> the kanji's radicals: what their tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<string> radicals) => All = radicals;

   void SetTheRadicals(IReadOnlyList<string> radicals) => _kanji.PublishIfTheListChanged(All, radicals, static it => new KanjiTevent.RadicalsUpdated(it));

   public override string ToString() => string.Join(", ", All);
}
