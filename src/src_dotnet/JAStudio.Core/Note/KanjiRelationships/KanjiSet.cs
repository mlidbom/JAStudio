using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A set of kanji a kanji stores, each named by its character, such as the kanji it's confused with (<see cref="MutualKanjiRelationship"/>): each kanji once,<br/>
/// in the order they were added. A change publishes the set's tevent with the whole new set, which the kanji applies (<see cref="Apply"/>).
/// </summary>
/// <remarks>
/// It changes only this kanji's set: what a change means beyond it, such as adding the kanji to the other side of a relationship that goes both ways,<br/>
/// is for the relationship holding the set to do.
/// </remarks>
sealed class KanjiSet
{
   readonly KanjiNote _owner;
   readonly Func<IReadOnlyList<string>, NoteTevent> _teventOf;

   /// <param name="owner">The kanji that stores the set, which publishes its tevent.</param>
   /// <param name="kanji">The kanji the kanji stores.</param>
   /// <param name="teventOf">The set's tevent: that its kanji are now those it's given.</param>
   public KanjiSet(KanjiNote owner, IEnumerable<string> kanji, Func<IReadOnlyList<string>, NoteTevent> teventOf)
   {
      _owner = owner;
      _teventOf = teventOf;
      Kanji = [.. kanji];
   }

   public IReadOnlyList<string> Kanji { get; private set; }

   public bool Contains(string kanji) => Kanji.Contains(kanji);

   public void Add(string kanji)
   {
      if(Contains(kanji)) return;
      SetTheKanji([.. Kanji, kanji]);
   }

   public void Remove(string kanji) => SetTheKanji([.. Kanji.Where(it => it != kanji)]);

   /// <summary>Adds <paramref name="kanji"/>, unless the set has it, publishing the tevent <paramref name="teventOf"/> makes of it and the new list, instead of the set's own.</summary>
   public void Add(string kanji, Func<string, IReadOnlyList<string>, NoteTevent> teventOf)
   {
      if(Contains(kanji)) return;
      SetTheKanji([.. Kanji, kanji], kanjiNow => teventOf(kanji, kanjiNow));
   }

   /// <summary>Removes <paramref name="kanji"/>, publishing the tevent <paramref name="teventOf"/> makes of it and the new list, instead of the set's own.</summary>
   public void Remove(string kanji, Func<string, IReadOnlyList<string>, NoteTevent> teventOf) =>
      SetTheKanji([.. Kanji.Where(it => it != kanji)], kanjiNow => teventOf(kanji, kanjiNow));

   /// <summary>Makes <paramref name="kanji"/> the set's kanji: what the set's tevent's applier does.</summary>
   public void Apply(IReadOnlyList<string> kanji) => Kanji = kanji;

   void SetTheKanji(IReadOnlyList<string> kanji) => SetTheKanji(kanji, _teventOf);

   void SetTheKanji(IReadOnlyList<string> kanji, Func<IReadOnlyList<string>, NoteTevent> teventOf) => _owner.PublishIfTheListChanged(Kanji, kanji, teventOf);

   public override string ToString() => string.Join(", ", Kanji);
}
