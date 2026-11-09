using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Vocabulary.EditableFields;

/// <summary>
/// Whether a vocab is usually written in kana (<see cref="VocabNoteUsuallyWrittenInKana"/>): undecided, which leaves it for JAStudio to decide<br/>
/// when it generates the vocab's data, yes, or no.
/// </summary>
public sealed class EditableUsuallyWrittenInKanaField : EditableField<VocabNote, UsuallyWrittenInKanaDecision>
{
   public EditableUsuallyWrittenInKanaField() : base("usuallyWrittenInKana", "Usually written in kana", changesHowSentencesParse: false) {}

   public override UsuallyWrittenInKanaDecision Read(VocabNote note) => note.UsuallyWrittenInKana.Decision;
   public override void Write(VocabNote note, UsuallyWrittenInKanaDecision value) => note.UsuallyWrittenInKana.Decide(value);
}
