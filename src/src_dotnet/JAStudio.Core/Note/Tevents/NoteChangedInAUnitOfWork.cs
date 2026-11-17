namespace JAStudio.Core.Note.Tevents;

public abstract class NoteChangedInAUnitOfWork(NoteId noteId) : INoteChangedInAUnitOfWork
{
   public NoteId NoteId { get; } = noteId;

   public class Vocab(NoteId vocabId) : NoteChangedInAUnitOfWork(vocabId), INoteChangedInAUnitOfWork.Vocab;
   public class Kanji(NoteId kanjiId) : NoteChangedInAUnitOfWork(kanjiId), INoteChangedInAUnitOfWork.Kanji;
   public class Sentence(NoteId sentenceId) : NoteChangedInAUnitOfWork(sentenceId), INoteChangedInAUnitOfWork.Sentence;
}
