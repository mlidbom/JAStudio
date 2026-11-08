namespace JAStudio.Core.Note.Vocabulary;

/// <summary>Whether a vocab is usually written in kana only, as its <see cref="VocabNoteUsuallyWrittenInKana"/> has it decided: yes, no, or not yet.</summary>
public enum UsuallyWrittenInKanaDecision
{
   /// <summary>Not decided yet, so JAStudio may decide it as <see cref="Yes"/> when it generates the vocab's data.</summary>
   Undecided,
   Yes,
   No
}
