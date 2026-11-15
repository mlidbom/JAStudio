using JAStudio.Core.Note.EditableFields;

namespace JAStudio.Core.Note.Sentences.ParsingCases;

/// <summary>
/// What a person has found about the vocab a sentence's parsing case expects the breakdown to display<br/>
/// (<see cref="ParsingCase.VerdictOnTheDisplayedVocab"/>): that they're right, that they're wrong, or nothing yet.
/// </summary>
/// <remarks>
/// A sentence that isn't a parsing case reads as <see cref="Verdict.Unverified"/>, and can't be given a verdict:<br/>
/// <see cref="SentenceCommands.MarkAsAParsingCase"/> makes it a parsing case first.
/// </remarks>
public sealed class EditableVerdictOnTheDisplayedVocabField : EditableField<SentenceNote, Verdict>
{
   public EditableVerdictOnTheDisplayedVocabField() : base("parsingCase.verdictOnTheDisplayedVocab", "Verdict on the displayed vocab", changesHowSentencesParse: false) {}

   public override Verdict Read(SentenceNote note) => note.ParsingCase?.VerdictOnTheDisplayedVocab ?? Verdict.Unverified;
   public override void Write(SentenceNote note, Verdict value) => note.SetTheVerdictOnTheDisplayedVocab(value);
}
