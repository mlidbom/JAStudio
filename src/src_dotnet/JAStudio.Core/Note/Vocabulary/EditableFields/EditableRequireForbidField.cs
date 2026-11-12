using System;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// One of a vocab's require/forbid rules outside its match-condition (<see cref="VocabMatchingRequireForbidFlags"/>), such as whether a word must start<br/>
/// with the dictionary form's ending: required, forbidden, or neither (<see cref="RequireForbidChoice"/>). Every change to one changes how sentences parse.
/// </summary>
public sealed class EditableRequireForbidField : EditableField<VocabNote, RequireForbidChoice>
{
   readonly Func<VocabNote, RequireForbidChoice> _read;
   readonly Action<VocabNote, RequireForbidChoice> _write;

   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>.</param>
   /// <param name="label">What the vocab's editor shows beside the rule's choices.</param>
   /// <param name="read">Reads the rule's choice on a vocab.</param>
   /// <param name="write">Makes a choice for the rule on a vocab, and saves the vocab.</param>
   public EditableRequireForbidField(string name, string label, Func<VocabNote, RequireForbidChoice> read, Action<VocabNote, RequireForbidChoice> write)
      : base(name, label, changesHowSentencesParse: true)
   {
      _read = read;
      _write = write;
   }

   public override RequireForbidChoice Read(VocabNote note) => _read(note);
   public override void Write(VocabNote note, RequireForbidChoice value) => _write(note, value);
}
