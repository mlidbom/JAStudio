using System;
using JAStudio.Core.Note.NoteFields;

namespace JAStudio.Core.Note.EditableFields;

/// <summary>A yes-or-no setting of a note, a <see cref="FlagField"/>, such as whether a vocab is a poison word: set or not.</summary>
public sealed class EditableFlagField<TNote> : EditableField<TNote, bool> where TNote : JPNote
{
   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: the path of the note's list of flags of its kind, then the flag's own name.</param>
   /// <param name="label">What the note's editor shows beside the check box.</param>
   /// <param name="flagOn">Finds the flag on a note.</param>
   /// <param name="changesHowSentencesParse">See <see cref="IEditableField{TNote}.ChangesHowSentencesParse"/>.</param>
   public EditableFlagField(string name, string label, Func<TNote, FlagField> flagOn, bool changesHowSentencesParse = false)
      : base(name, label, changesHowSentencesParse) =>
      FlagOn = flagOn;

   /// <summary>Finds the flag on a note, for the editor's check box, which reads and writes it there.</summary>
   public Func<TNote, FlagField> FlagOn { get; }

   public override bool Read(TNote note) => FlagOn(note).IsSet();
   public override void Write(TNote note, bool value) => FlagOn(note).SetTo(value);
}
