using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note that holds a set of items in no order, such as a vocab's synonyms, which a person changes one item at a time: the note has<br/>
/// an operation that adds an item and one that removes it (<see cref="Add"/>, <see cref="Remove"/>). Its value is the items in a fixed order.
/// </summary>
/// <remarks>
/// Writing a whole set (<see cref="Write"/>) adds each item the note's set lacks and removes each item it has that isn't in the new set, through those<br/>
/// same operations, so whatever they do besides, such as adding the vocab to its new synonym's synonyms too, happens as when a person makes the change.
/// </remarks>
public class EditableSetField<TNote, TItem> : EditableField<TNote, IReadOnlyList<TItem>> where TNote : JPNote where TItem : notnull
{
   readonly Func<TNote, IEnumerable<TItem>> _itemsOn;
   readonly Action<TNote, TItem> _add;
   readonly Action<TNote, TItem> _remove;
   readonly Func<IEnumerable<TItem>, IEnumerable<TItem>> _inOrder;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows above the set.</param>
   /// <param name="itemsOn">Reads the items of the set on a note.</param>
   /// <param name="add">Adds an item to the set on a note, and saves the note.</param>
   /// <param name="remove">Removes an item from the set on a note, and saves the note.</param>
   /// <param name="inOrder">Puts items in the order they have in the field's value.</param>
   /// <param name="changesHowSentencesParse">See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</param>
   public EditableSetField(string name, string label, Func<TNote, IEnumerable<TItem>> itemsOn, Action<TNote, TItem> add, Action<TNote, TItem> remove, Func<IEnumerable<TItem>, IEnumerable<TItem>> inOrder, bool changesHowSentencesParse = false)
      : base(name, label, changesHowSentencesParse)
   {
      _itemsOn = itemsOn;
      _add = add;
      _remove = remove;
      _inOrder = inOrder;
   }

   /// <summary>Adds <paramref name="item"/> to the set on <paramref name="note"/>, and saves the note.</summary>
   public void Add(TNote note, TItem item) => _add(note, item);

   /// <summary>Removes <paramref name="item"/> from the set on <paramref name="note"/>, and saves the note.</summary>
   public void Remove(TNote note, TItem item) => _remove(note, item);

   public override IReadOnlyList<TItem> Read(TNote note) => [.. _inOrder(_itemsOn(note))];

   public override void Write(TNote note, IReadOnlyList<TItem> value)
   {
      var itemsNow = Read(note);
      foreach(var item in itemsNow.Except(value)) Remove(note, item);
      foreach(var item in value.Except(itemsNow)) Add(note, item);
   }
}
