using System.Collections.Generic;
using System.Linq;
using JAStudio.Core.Note.Collection;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A kanji's relationship to other kanji, such as the kanji it's confused with (<see cref="KanjiNote.ConfusedWith"/>): the <see cref="Kanji"/>, each named<br/>
/// by its character, in the order they were added. It goes one way: the other kanji don't know of it. <see cref="MutualKanjiRelationship"/> goes both ways.
/// </summary>
/// <remarks>
/// A kanji is named by its character, which never changes, so the character is the reference: a kanji the corpus has no note for can be named too.
/// </remarks>
public class KanjiRelationship
{
   readonly KanjiSet _kanji;

   /// <param name="owner">The kanji whose relationship this is.</param>
   /// <param name="kanji">The kanji it has the relationship to.</param>
   /// <param name="guard">Saves the kanji when the relationship changes.</param>
   public KanjiRelationship(KanjiNote owner, IEnumerable<string> kanji, NoteGuard guard)
   {
      Owner = owner;
      _kanji = new KanjiSet(kanji, guard);
   }

   /// <summary>The kanji whose relationship this is.</summary>
   protected KanjiNote Owner { get; }

   protected KanjiCollection Collection => Owner.Services.Collection.Kanji;

   /// <summary>The kanji the <see cref="Owner"/> has this relationship to, in the order they were added.</summary>
   public IReadOnlyList<string> Kanji => _kanji.Kanji;

   public bool Contains(string kanji) => _kanji.Contains(kanji);

   /// <summary>The notes of the <see cref="Kanji"/> that the collection has.</summary>
   public List<KanjiNote> Notes() => [.. Kanji.Select(Collection.WithKanji).OfType<KanjiNote>()];

   /// <summary>Adds <paramref name="kanji"/>. A kanji has no relationship to itself, so adding its own character does nothing.</summary>
   public virtual void Add(string kanji)
   {
      if(Owner.IsNamedBy(kanji)) return;
      AddTheKanji(kanji);
   }

   public virtual void Remove(string kanji) => RemoveTheKanji(kanji);

   /// <summary>Adds <paramref name="kanji"/> to this kanji's side of the relationship only.</summary>
   protected void AddTheKanji(string kanji) => _kanji.Add(kanji);

   /// <summary>Removes <paramref name="kanji"/> from this kanji's side of the relationship only.</summary>
   protected void RemoveTheKanji(string kanji) => _kanji.Remove(kanji);

   public override string ToString() => _kanji.ToString();
}
