using System;
using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A kanji's relationship to other kanji, such as the kanji with a similar meaning (<see cref="KanjiNote.SimilarMeaning"/>) and the kanji it's confused<br/>
/// with (<see cref="KanjiNote.ConfusedWith"/>): the <see cref="Kanji"/>, each named by its character, in the order they were added. It goes both ways:<br/>
/// adding a kanji adds this kanji to the same relationship of that kanji's note, and removing one removes it there too.
/// </summary>
/// <remarks>
/// A kanji is named by its character, which never changes, so the character is the reference: a kanji the corpus has no note for can be named too,<br/>
/// and then has no other side, so only this kanji's side changes. Unlike a vocab's relationships, every relationship of a kanji goes both ways<br/>
/// (Magnus, 2026-11-10).
/// </remarks>
public class MutualKanjiRelationship
{
   readonly KanjiSet _kanji;
   readonly Func<KanjiNote, MutualKanjiRelationship> _theSameRelationshipOf;

   /// <param name="owner">The kanji whose relationship this is.</param>
   /// <param name="kanji">The kanji it has the relationship to.</param>
   /// <param name="guard">Saves the kanji when the relationship changes.</param>
   /// <param name="theSameRelationshipOf">The same relationship of another kanji, such as <c>it => it.SimilarMeaning</c>.</param>
   public MutualKanjiRelationship(KanjiNote owner, IEnumerable<string> kanji, NoteGuard guard, Func<KanjiNote, MutualKanjiRelationship> theSameRelationshipOf)
   {
      Owner = owner;
      _kanji = new KanjiSet(kanji, guard);
      _theSameRelationshipOf = theSameRelationshipOf;
   }

   /// <summary>The kanji whose relationship this is.</summary>
   KanjiNote Owner { get; }

   KanjiCollection Collection => Owner.Services.Collection.Kanji;

   /// <summary>The kanji the <see cref="Owner"/> has this relationship to, in the order they were added.</summary>
   public IReadOnlyList<string> Kanji => _kanji.Kanji;

   public bool Contains(string kanji) => _kanji.Contains(kanji);

   /// <summary>The notes of the <see cref="Kanji"/> that the collection has.</summary>
   public List<KanjiNote> Notes() => [.. Kanji.Select(Collection.WithKanji).OfType<KanjiNote>()];

   /// <summary>
   /// Adds <paramref name="kanji"/>, and this kanji to the same relationship of <paramref name="kanji"/>'s note. A kanji has no relationship to itself,<br/>
   /// so adding its own character does nothing.
   /// </summary>
   public void Add(string kanji)
   {
      if(Owner.IsNamedBy(kanji)) return;

      _kanji.Add(kanji);
      OtherSideOf(kanji)?._kanji.Add(Owner.GetQuestion());
   }

   /// <summary>Removes <paramref name="kanji"/>, and this kanji from the same relationship of <paramref name="kanji"/>'s note.</summary>
   public void Remove(string kanji)
   {
      _kanji.Remove(kanji);
      OtherSideOf(kanji)?._kanji.Remove(Owner.GetQuestion());
   }

   /// <summary>The same relationship of <paramref name="kanji"/>'s note, or null when the collection has no note for it.</summary>
   MutualKanjiRelationship? OtherSideOf(string kanji) => Collection.WithKanji(kanji) is {} other ? _theSameRelationshipOf(other) : null;

   public override string ToString() => _kanji.ToString();
}
