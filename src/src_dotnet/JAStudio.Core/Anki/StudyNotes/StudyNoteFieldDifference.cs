namespace JAStudio.Core.Anki.StudyNotes;

/// <summary>A field of a study-note whose value in Anki isn't what JAStudio would write.</summary>
/// <param name="Field">The field's name in Anki, such as <see cref="AnkiFieldNames.Question"/>.</param>
/// <param name="InAnki">The field's value in Anki, or null when the note type in Anki has no such field.</param>
/// <param name="JAStudioWrites">The value JAStudio would write.</param>
public record StudyNoteFieldDifference(string Field, string? InAnki, string JAStudioWrites);
