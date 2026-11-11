using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note that holds a set of texts, in no order, such as a vocab's "prefix is not" matching rule or its synonyms: an<br/>
/// <see cref="EditableSetField{TNote,TItem}"/> of texts, whose value is the texts in ordinal order.
/// </summary>
public sealed class EditableTextSetField<TNote> : EditableSetField<TNote, string> where TNote : JPNote
{
   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows above the set.</param>
   /// <param name="textsOn">Reads the texts of the set on a note.</param>
   /// <param name="add">Adds a text to the set on a note, and saves the note.</param>
   /// <param name="remove">Removes a text from the set on a note, and saves the note.</param>
   /// <param name="changesHowSentencesParse">See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</param>
   public EditableTextSetField(string name, string label, Func<TNote, IEnumerable<string>> textsOn, Action<TNote, string> add, Action<TNote, string> remove, bool changesHowSentencesParse = false)
      : base(name, label, textsOn, add, remove, texts => texts.Order(StringComparer.Ordinal), changesHowSentencesParse) {}
}
