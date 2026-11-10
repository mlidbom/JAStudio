using System.Collections.Generic;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A set of kanji a kanji stores, each named by its character, such as the kanji it's confused with (<see cref="MutualKanjiRelationship"/>): each kanji once,<br/>
/// in the order they were added. Every change saves the kanji, through its <see cref="NoteGuard"/>.
/// </summary>
/// <remarks>
/// It changes only this kanji's set: what a change means beyond it, such as adding the kanji to the other side of a relationship that goes both ways,<br/>
/// is for the relationship holding the set to do.
/// </remarks>
sealed class KanjiSet
{
   readonly List<string> _kanji;
   readonly NoteGuard _guard;

   /// <param name="kanji">The kanji the kanji stores.</param>
   /// <param name="guard">Saves the kanji when the set changes.</param>
   public KanjiSet(IEnumerable<string> kanji, NoteGuard guard)
   {
      _kanji = [.. kanji];
      _guard = guard;
   }

   public IReadOnlyList<string> Kanji => _kanji;

   public bool Contains(string kanji) => _kanji.Contains(kanji);

   public void Add(string kanji)
   {
      if(Contains(kanji)) return;
      _guard.Update(() => _kanji.Add(kanji));
   }

   public void Remove(string kanji)
   {
      if(!Contains(kanji)) return;
      _guard.Update(() => _kanji.Remove(kanji));
   }

   public override string ToString() => string.Join(", ", _kanji);
}
