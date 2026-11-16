using JAStudio.Core.Note.Tevents;

namespace JAStudio.Core.Note.KanjiTevents;

public abstract class KanjiTevent : NoteTevent, IKanjiTevent
{
   KanjiTevent(KanjiId kanjiId) : base(kanjiId) {}

   public class Created(KanjiId kanjiId, string question) : KanjiTevent(kanjiId), IKanjiTevent.Created
   {
      public string Question { get; } = question;
   }
}

public class KanjiTevent<TTevent>(TTevent tevent) : NoteTevent<TTevent>(tevent), IKanjiTevent<TTevent> where TTevent : INoteTevent;
