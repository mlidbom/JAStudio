using JAStudio.Core.Storage.Dto;

namespace JAStudio.Core.Note.Vocabulary;

public class VocabNoteMetaData
{
   readonly VocabNote _vocab;
   readonly NoteGuard _guard;

   public VocabNoteMetaData(VocabNote vocab, VocabDto? data, NoteGuard guard)
   {
      _vocab = vocab;
      _guard = guard;
      SentenceCount = data?.SentenceCount ?? 0;
   }

   public int SentenceCount { get; private set; }

   public void SetSentenceCount(int value) => _guard.Update(() => SentenceCount = value);

   public string MetaTagsHtml(bool displayExtendedSentenceStatistics = true, bool noSentenceStatistics = false) => VocabNoteMetaTagFormatter.GetMetaTagsHtml(_vocab, displayExtendedSentenceStatistics, noSentenceStatistics);
}
