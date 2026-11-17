using System;
using System.Collections.Generic;
using System.Linq;
using Compze.Tessaging.TessageBus;
using JAStudio.Core.Note.Collection;
using JAStudio.Core.Note.KanjiTevents;

namespace JAStudio.Core.Note.KanjiRelationships;

/// <summary>
/// A kanji's relationship to other kanji, such as the kanji with a similar meaning (<see cref="KanjiNote.SimilarMeaning"/>) and the kanji it's confused<br/>
/// with (<see cref="KanjiNote.ConfusedWith"/>): the <see cref="Kanji"/>, each named by its character, in the order they were added. It goes both ways:<br/>
/// adding a kanji adds this kanji to the same relationship of that kanji's note, and removing one removes it there too. A participation handler of the<br/>
/// tevent of the adding or the removing does that (<see cref="MirrorTheRelationshipsThrough"/>).
/// </summary>
/// <remarks>
/// A kanji is named by its character, which never changes, so the character is the reference: a kanji the corpus has no note for can be named too,<br/>
/// and then has no other side, so only this kanji's side changes. Unlike a vocab's relationships, every relationship of a kanji goes both ways<br/>
/// (Magnus, 2026-11-10).
/// </remarks>
public class MutualKanjiRelationship
{
   static readonly MutualKanjiRelationshipTevents SimilarMeaningTevents = new(
      static kanjiNow => new KanjiTevent.SimilarMeaningUpdated(kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.SimilarMeaningAdded(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.SimilarMeaningRemoved(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.SimilarMeaningAddedAsTheMirrorOf(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.SimilarMeaningRemovedAsTheMirrorOf(kanji, kanjiNow));

   static readonly MutualKanjiRelationshipTevents ConfusedWithTevents = new(
      static kanjiNow => new KanjiTevent.ConfusedWithUpdated(kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.ConfusedWithAdded(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.ConfusedWithRemoved(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.ConfusedWithAddedAsTheMirrorOf(kanji, kanjiNow),
      static (kanji, kanjiNow) => new KanjiTevent.ConfusedWithRemovedAsTheMirrorOf(kanji, kanjiNow));

   /// <summary>
   /// Declares, through <paramref name="handle"/>, the participation handlers by which a kanji's relationships are mirrored, in the unit of work that<br/>
   /// changed them: a kanji added to a kanji's relationship adds the kanji to the same relationship of the kanji added, and a kanji removed removes it<br/>
   /// (<see cref="MirrorTheAdding"/>, <see cref="MirrorTheRemoving"/>).
   /// </summary>
   internal static void MirrorTheRelationshipsThrough(TessageBusHandlerRegistrar handle) => handle
     .ForTevent(static (IKanjiTevent.PropertyUpdated.SimilarMeaning.Added it, JPCollection collection) =>
         collection.TheRunningUnitOfWork.NoteThatPublished<KanjiNote>(it).SimilarMeaning.MirrorTheAdding(it.Kanji))
     .ForTevent(static (IKanjiTevent.PropertyUpdated.SimilarMeaning.Removed it, JPCollection collection) =>
         collection.TheRunningUnitOfWork.NoteThatPublished<KanjiNote>(it).SimilarMeaning.MirrorTheRemoving(it.Kanji))
     .ForTevent(static (IKanjiTevent.PropertyUpdated.ConfusedWith.Added it, JPCollection collection) =>
         collection.TheRunningUnitOfWork.NoteThatPublished<KanjiNote>(it).ConfusedWith.MirrorTheAdding(it.Kanji))
     .ForTevent(static (IKanjiTevent.PropertyUpdated.ConfusedWith.Removed it, JPCollection collection) =>
         collection.TheRunningUnitOfWork.NoteThatPublished<KanjiNote>(it).ConfusedWith.MirrorTheRemoving(it.Kanji));

   /// <summary>A kanji's kanji with a similar meaning (<see cref="KanjiNote.SimilarMeaning"/>).</summary>
   internal static MutualKanjiRelationship SimilarMeaningOf(KanjiNote owner, IEnumerable<string> kanji) => new(owner, kanji, SimilarMeaningTevents, static it => it.SimilarMeaning);

   /// <summary>The kanji a kanji is confused with (<see cref="KanjiNote.ConfusedWith"/>).</summary>
   internal static MutualKanjiRelationship ConfusedWithOf(KanjiNote owner, IEnumerable<string> kanji) => new(owner, kanji, ConfusedWithTevents, static it => it.ConfusedWith);

   readonly KanjiSet _kanji;
   readonly MutualKanjiRelationshipTevents _tevents;
   readonly Func<KanjiNote, MutualKanjiRelationship> _theSameRelationshipOf;

   /// <param name="owner">The kanji whose relationship this is.</param>
   /// <param name="kanji">The kanji it has the relationship to.</param>
   /// <param name="tevents">The relationship's tevents.</param>
   /// <param name="theSameRelationshipOf">The same relationship of another kanji, such as <c>it => it.SimilarMeaning</c>.</param>
   MutualKanjiRelationship(KanjiNote owner, IEnumerable<string> kanji, MutualKanjiRelationshipTevents tevents, Func<KanjiNote, MutualKanjiRelationship> theSameRelationshipOf)
   {
      Owner = owner;
      _kanji = new KanjiSet(owner, kanji, tevents.Updated);
      _tevents = tevents;
      _theSameRelationshipOf = theSameRelationshipOf;
   }

   /// <summary>The kanji whose relationship this is.</summary>
   KanjiNote Owner { get; }

   KanjiCollection Collection => Owner.Services.Collection.Kanji;

   /// <summary>The kanji the <see cref="Owner"/> has this relationship to, in the order they were added.</summary>
   public IReadOnlyList<string> Kanji => _kanji.Kanji;

   public bool Contains(string kanji) => _kanji.Contains(kanji);

   /// <summary>Makes <paramref name="kanji"/> the kanji the <see cref="Owner"/> has this relationship to: what the relationship's tevent's applier does.</summary>
   internal void Apply(IReadOnlyList<string> kanji) => _kanji.Apply(kanji);

   /// <summary>The notes of the <see cref="Kanji"/> that the collection has.</summary>
   public List<KanjiNote> Notes() => [.. Kanji.Select(Collection.WithKanji).OfType<KanjiNote>()];

   /// <summary>
   /// Adds <paramref name="kanji"/>, and so this kanji to the same relationship of <paramref name="kanji"/>'s note (<see cref="MirrorTheAdding"/>). A kanji<br/>
   /// has no relationship to itself, so adding its own character does nothing.
   /// </summary>
   public void Add(string kanji)
   {
      if(Owner.IsNamedBy(kanji)) return;
      _kanji.Add(kanji, _tevents.Added);
   }

   /// <summary>Removes <paramref name="kanji"/>, and so this kanji from the same relationship of <paramref name="kanji"/>'s note (<see cref="MirrorTheRemoving"/>).</summary>
   public void Remove(string kanji) => _kanji.Remove(kanji, _tevents.Removed);

   /// <summary>
   /// What a participation handler of the adding of <paramref name="kanji"/> does: adds this kanji to the same relationship of <paramref name="kanji"/>'s<br/>
   /// note, as the mirror of the adding, which nothing mirrors back.
   /// </summary>
   void MirrorTheAdding(string kanji) => ChangeTheOtherSideOf(kanji, it => it._kanji.Add(Owner.GetQuestion(), it._tevents.AddedAsTheMirrorOf));

   /// <summary>
   /// What a participation handler of the removing of <paramref name="kanji"/> does: removes this kanji from the same relationship of<br/>
   /// <paramref name="kanji"/>'s note, as the mirror of the removing, which nothing mirrors back.
   /// </summary>
   void MirrorTheRemoving(string kanji) => ChangeTheOtherSideOf(kanji, it => it._kanji.Remove(Owner.GetQuestion(), it._tevents.RemovedAsTheMirrorOf));

   /// <summary>
   /// Changes, with <paramref name="change"/>, the same relationship of <paramref name="kanji"/>'s note, in an edit of that note (<see cref="EditingANote"/>),<br/>
   /// when the collection has a note for it.
   /// </summary>
   void ChangeTheOtherSideOf(string kanji, Action<MutualKanjiRelationship> change)
   {
      if(Collection.WithKanji(kanji) is {} otherKanji) otherKanji.Edit(it => change(_theSameRelationshipOf(it)));
   }

   public override string ToString() => _kanji.ToString();
}
