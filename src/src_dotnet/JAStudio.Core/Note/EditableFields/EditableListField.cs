using System;
using System.Collections.Generic;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note that holds a list of items in an order that matters, where an item may come more than once, such as a kanji's radicals, which a<br/>
/// person changes one item at a time: the note's editor adds an item after the others, and removes one. Writing a whole list replaces the note's.
/// </summary>
public sealed class EditableListField<TNote, TItem> : EditableField<TNote, IReadOnlyList<TItem>>, IFieldEditedOneItemAtATime<TNote, TItem> where TNote : JPNote where TItem : notnull
{
   readonly Func<TNote, IReadOnlyList<TItem>> _itemsOn;
   readonly Action<TNote, TItem> _add;
   readonly Action<TNote, TItem> _remove;
   readonly Action<TNote, IReadOnlyList<TItem>> _set;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows above the list.</param>
   /// <param name="itemsOn">Reads the list on a note.</param>
   /// <param name="add">Adds an item after the others on a note, and saves the note.</param>
   /// <param name="remove">Removes an item once from a note, and saves the note.</param>
   /// <param name="set">Replaces the list on a note, and saves the note.</param>
   public EditableListField(string name, string label, Func<TNote, IReadOnlyList<TItem>> itemsOn, Action<TNote, TItem> add, Action<TNote, TItem> remove, Action<TNote, IReadOnlyList<TItem>> set)
      : base(name, label, changesHowSentencesParse: false)
   {
      _itemsOn = itemsOn;
      _add = add;
      _remove = remove;
      _set = set;
   }

   public bool HoldsEachItemOnce => false;

   public void Add(TNote note, TItem item) => _add(note, item);

   public void Remove(TNote note, TItem item) => _remove(note, item);

   public override IReadOnlyList<TItem> Read(TNote note) => _itemsOn(note);

   public override void Write(TNote note, IReadOnlyList<TItem> value) => _set(note, value);
}
