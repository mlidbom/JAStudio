namespace JAStudio.Core.Note.Collection;

public class CardStudyingStatus
{
   public long StudyNoteId { get; }
   public string CardType { get; }
   public bool IsSuspended { get; }
   public string NoteTypeName { get; }

   public CardStudyingStatus(long studyNoteId, string cardType, bool isSuspended, string noteTypeName)
   {
      StudyNoteId = studyNoteId;
      CardType = cardType;
      IsSuspended = isSuspended;
      NoteTypeName = noteTypeName;
   }
}
