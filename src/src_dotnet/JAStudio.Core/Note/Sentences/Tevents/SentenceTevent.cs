using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.Sentences.Tevents;

public abstract class SentenceTevent : NoteTevent, ISentenceTevent
{
   SentenceTevent(SentenceId sentenceId) : base(sentenceId) {}

   public class Created(SentenceId sentenceId) : SentenceTevent(sentenceId), ISentenceTevent.Created;
}

public class SentenceTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), ISentenceTevent<TTevent> where TTevent : INoteTevent;
