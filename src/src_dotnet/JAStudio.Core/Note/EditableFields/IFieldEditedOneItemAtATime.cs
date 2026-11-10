using System.Collections.Generic;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note holding several items, which a person changes one item at a time in the note's editor, such as a kanji's radicals or the kanji it's<br/>
/// confused with: each item shown with a button that removes it, and a box that adds one.
/// </summary>
public interface IFieldEditedOneItemAtATime<in TNote, TItem> where TNote : JPNote
{
   /// <summary>What the note's editor shows above the items.</summary>
   string Label { get; }

   /// <summary>Whether the field holds each item at most once, as a set does, so that adding an item it holds already does nothing.</summary>
   bool HoldsEachItemOnce { get; }

   /// <summary>The items on <paramref name="note"/>, in the order the editor shows them.</summary>
   IReadOnlyList<TItem> Read(TNote note);

   /// <summary>Adds <paramref name="item"/> to the field on <paramref name="note"/>, and saves the note.</summary>
   void Add(TNote note, TItem item);

   /// <summary>Removes <paramref name="item"/> from the field on <paramref name="note"/>, and saves the note.</summary>
   void Remove(TNote note, TItem item);
}
