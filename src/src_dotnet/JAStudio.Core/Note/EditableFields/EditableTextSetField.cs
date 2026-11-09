using System;
using System.Collections.Generic;
using System.Linq;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>
/// A field of a note that holds a set of texts, in no order, such as a vocab's "prefix is not" matching rule. The note's editor adds and removes<br/>
/// one text at a time (<see cref="Add"/>, <see cref="Remove"/>); its value is the texts in ordinal order.
/// </summary>
public sealed class EditableTextSetField<TNote> : EditableField<TNote, IReadOnlyList<string>> where TNote : JPNote
{
   readonly Func<TNote, HashSet<string>> _setOn;
   readonly Action<TNote> _save;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the note as the corpus stores it.</param>
   /// <param name="label">What the note's editor shows above the set.</param>
   /// <param name="setOn">Finds the set on a note. Changing it saves nothing by itself.</param>
   /// <param name="save">Saves a note after its set changed.</param>
   /// <param name="changesHowSentencesParse">See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</param>
   public EditableTextSetField(string name, string label, Func<TNote, HashSet<string>> setOn, Action<TNote> save, bool changesHowSentencesParse = false)
      : base(name, label, changesHowSentencesParse)
   {
      _setOn = setOn;
      _save = save;
   }

   /// <summary>Adds <paramref name="text"/> to the set on <paramref name="note"/>, and saves the note.</summary>
   public void Add(TNote note, string text) => ChangeAndSave(note, set => set.Add(text));

   /// <summary>Removes <paramref name="text"/> from the set on <paramref name="note"/>, and saves the note.</summary>
   public void Remove(TNote note, string text) => ChangeAndSave(note, set => set.Remove(text));

   public override IReadOnlyList<string> Read(TNote note) => [.. _setOn(note).Order(StringComparer.Ordinal)];

   public override void Write(TNote note, IReadOnlyList<string> value) => ChangeAndSave(note, set =>
   {
      set.Clear();
      set.UnionWith(value);
   });

   void ChangeAndSave(TNote note, Action<HashSet<string>> change)
   {
      change(_setOn(note));
      _save(note);
   }
}
