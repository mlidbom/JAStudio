using JAStudio.Core.Anki;

namespace JAStudio.Core.Studying.StudyNotes;

/// <summary>A field of a study-note whose value in the SRS, the spaced repetition system, isn't what JAStudio would write.</summary>
/// <param name="Field">The field's name in the spaced repetition system: in Anki, such as <see cref="AnkiFieldNames.Question"/>.</param>
/// <param name="InTheSRS">The field's value in the spaced repetition system, or null when the study-note there has no such field.</param>
/// <param name="JAStudioWrites">The value JAStudio would write.</param>
/// <remarks>review-naming:studying: InTheSRS was InAnki (Claude, 2026-11-15).</remarks>
public record StudyNoteFieldDifference(string Field, string? InTheSRS, string JAStudioWrites);
