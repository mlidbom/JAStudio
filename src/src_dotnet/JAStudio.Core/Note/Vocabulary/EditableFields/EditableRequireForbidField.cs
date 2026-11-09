using System;
using JAStudio.Core.Note.EditableFields;
using JAStudio.Core.Note.Vocabulary.Matching;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// One of a vocab's require/forbid matching rules (<see cref="RequireForbidFlagField"/>), such as whether a word must be its surface form to match:<br/>
/// required, forbidden, or neither (<see cref="RequireForbidChoice"/>). Every change to one changes how sentences parse.
/// </summary>
public sealed class EditableRequireForbidField : EditableField<VocabNote, RequireForbidChoice>
{
   /// <param name="name">The field's <see cref="IEditableField{TNote}.Name"/>: its path in the vocab as the corpus stores it.</param>
   /// <param name="label">What the vocab's editor shows beside the rule's choices.</param>
   /// <param name="ruleOn">Finds the rule on a vocab.</param>
   public EditableRequireForbidField(string name, string label, Func<VocabNote, RequireForbidFlagField> ruleOn)
      : base(name, label, changesHowSentencesParse: true) =>
      RuleOn = ruleOn;

   /// <summary>Finds the rule on a vocab, for the editor's choices, which read and write it there.</summary>
   public Func<VocabNote, RequireForbidFlagField> RuleOn { get; }

   public override RequireForbidChoice Read(VocabNote note) => RuleOn(note).Choice;
   public override void Write(VocabNote note, RequireForbidChoice value) => RuleOn(note).Choose(value);
}
