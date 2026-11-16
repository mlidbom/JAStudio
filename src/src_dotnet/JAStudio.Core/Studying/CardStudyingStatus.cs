using JAStudio.Core.Note;

namespace JAStudio.Core.Studying;

/// <summary>
/// The studying status of one card of a study-note, as the spaced repetition system has it: which of the study-note's cards it is, and whether<br/>
/// it's suspended. A note is studied through a card that isn't suspended.
/// </summary>
public class CardStudyingStatus
{
   /// <summary>The id of the study-note the card belongs to (<see cref="StudyNoteIds"/>).</summary>
   public long StudyNoteId { get; }

   /// <summary>Which of the study-note's cards it is, such as <see cref="CardTypes.Reading"/>.</summary>
   public string CardType { get; }

   public bool IsSuspended { get; }

   /// <summary>The name of the study-note's note type, such as <see cref="NoteTypes.Vocab"/>.</summary>
   public string NoteTypeName { get; }

   public CardStudyingStatus(long studyNoteId, string cardType, bool isSuspended, string noteTypeName)
   {
      StudyNoteId = studyNoteId;
      CardType = cardType;
      IsSuspended = isSuspended;
      NoteTypeName = noteTypeName;
   }
}
