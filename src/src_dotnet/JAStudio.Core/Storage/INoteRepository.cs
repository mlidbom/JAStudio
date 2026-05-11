using JAStudio.Core.Note;
using JAStudio.Core.Note.Sentences;
using JAStudio.Core.Note.Vocabulary;

namespace JAStudio.Core.Storage;

public interface INoteRepository
{
   void Save(KanjiNote note);
   void Save(VocabNote note);
   void Save(SentenceNote note);

   void Delete(NoteId noteId);

   AllNotesData LoadAll();
}
