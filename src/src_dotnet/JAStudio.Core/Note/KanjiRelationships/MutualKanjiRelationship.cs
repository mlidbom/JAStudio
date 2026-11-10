using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A kanji's relationship to other kanji that goes both ways, such as the kanji with a similar meaning (<see cref="KanjiNote.SimilarMeaning"/>): adding a<br/>
/// kanji adds this kanji to the same relationship of that kanji's note, and removing one removes it there too.
/// </summary>
/// <remarks>
/// A kanji the collection has no note for has no other side: only this kanji's side changes.
/// </remarks>
public class MutualKanjiRelationship : KanjiRelationship
{
   readonly Func<KanjiNote, MutualKanjiRelationship> _theSameRelationshipOf;

   /// <param name="owner">The kanji whose relationship this is.</param>
   /// <param name="kanji">The kanji it has the relationship to.</param>
   /// <param name="guard">Saves the kanji when the relationship changes.</param>
   /// <param name="theSameRelationshipOf">The same relationship of another kanji, such as <c>it => it.SimilarMeaning</c>.</param>
   public MutualKanjiRelationship(KanjiNote owner, IEnumerable<string> kanji, NoteGuard guard, Func<KanjiNote, MutualKanjiRelationship> theSameRelationshipOf)
      : base(owner, kanji, guard) => _theSameRelationshipOf = theSameRelationshipOf;

   /// <summary>Adds <paramref name="kanji"/>, and this kanji to the same relationship of <paramref name="kanji"/>'s note.</summary>
   public override void Add(string kanji)
   {
      if(Owner.IsNamedBy(kanji)) return;

      AddTheKanji(kanji);
      OtherSideOf(kanji)?.AddTheKanji(Owner.GetQuestion());
   }

   /// <summary>Removes <paramref name="kanji"/>, and this kanji from the same relationship of <paramref name="kanji"/>'s note.</summary>
   public override void Remove(string kanji)
   {
      RemoveTheKanji(kanji);
      OtherSideOf(kanji)?.RemoveTheKanji(Owner.GetQuestion());
   }

   /// <summary>The same relationship of <paramref name="kanji"/>'s note, or null when the collection has no note for it.</summary>
   MutualKanjiRelationship? OtherSideOf(string kanji) => Collection.WithKanji(kanji) is {} other ? _theSameRelationshipOf(other) : null;
}
